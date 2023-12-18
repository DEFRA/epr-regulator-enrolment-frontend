using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

using FrontendRegulatorAccountEnrollment.Core.Configs.Models;
using FrontendRegulatorAccountEnrollment.Core.Constants;
using FrontendRegulatorAccountEnrollment.Core.Models.Requests;
using FrontendRegulatorAccountEnrollment.Core.Models.Responses;
using FrontendRegulatorAccountEnrollment.Core.Services;
using FrontendRegulatorAccountEnrollment.Web.Configs;
using FrontendRegulatorAccountEnrollment.Web.Constants;
using FrontendRegulatorAccountEnrollment.Web.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace FrontendRegulatorAccountEnrollment.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFacadeService _facadeService;
        private readonly IAllowlistService _allowlistService;
        private readonly AppConfig _configuration;

        public HomeController(
            ILogger<HomeController> logger,
            IFacadeService facadeService,
            IAllowlistService allowlistService,
            IOptions<AppConfig> configuration

            )
        {
            _logger = logger;
            _facadeService = facadeService;
            _allowlistService = allowlistService;
            _configuration = configuration.Value;
        }

        [ActionName("InviteUser")]
        [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
        public async Task<RedirectResult> InviteUser()
        {
            string inviteToken = string.Empty;
            string b2cUserId = string.Empty;
            string nationName = string.Empty;
            Nation nationLookup = new Nation();
            OrganisationModel regulatorOrganisationModel = new OrganisationModel();
            bool orgExists = false;

            try
            {
                string? email = HttpContext.User.Claims.Single(claim => claim.Type == ClaimTypes.Email).Value.ToString().Trim().ToLower(CultureInfo.CurrentCulture);
                b2cUserId = HttpContext.User.Claims.Single(claim => claim.Type == ClaimConstants.ObjectId).Value;
                var regulatorEmailAllowModel = _allowlistService.CheckEmailIsAllowListed(email);

                if (regulatorEmailAllowModel is not null && !string.IsNullOrWhiteSpace(regulatorEmailAllowModel.KeyName))
                {
                    regulatorOrganisationModel = _allowlistService.QueryRegulatorOrganisationDetails(regulatorEmailAllowModel.KeyName);
                    nationName = nationLookup.GetNationName(regulatorOrganisationModel.NationId);
                }

                if (!IsDataValid(email, b2cUserId, regulatorEmailAllowModel, regulatorOrganisationModel))
                {
                    throw new InvalidDataException(ConfigConstants.IncorrectEmail);
                }

                var existingOrganisation = await _facadeService.CheckOrganisationExistsForNation(nationName);

                if (existingOrganisation.ExternalId != Guid.Empty)
                {
                    orgExists = true;
                }

                if (!orgExists)
                {
                    inviteToken = CreateNewEnrolment(regulatorOrganisationModel.KeyValue, regulatorOrganisationModel.NationId, email, b2cUserId);
                }

                else
                {
                    inviteToken = RetrieveExistingEnrolment(email);
                }

                if (string.IsNullOrWhiteSpace(inviteToken))
                {
                    _logger.LogInfo(ConfigConstants.FailedRetrive);
                    throw new InvalidOperationException(ConfigConstants.OrganisationExistsWithoutEnrolement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrors(ex);
                _logger.LogInfo($"Failed to redirect to Portal for UserId {b2cUserId}");
                return Redirect(PagePath.ErrorUrl);
            }

            _logger.LogInfo($"Redirect to Portal for UserId {b2cUserId}");
            _logger.LogInfo($"Full URL = {_configuration.BaseUrl}?token={inviteToken}");
            return RedirectPermanent($"{_configuration.BaseUrl}?token={inviteToken}");
        }

        [Route("error")]
        public IActionResult Error() => View("Error");

        private bool IsDataValid(string? email, string? b2cUserId, EmailModel? regulatorEmailAllowModel, OrganisationModel? regulatorOrganisationModel)
        {
            if (string.IsNullOrWhiteSpace(email))
            {

                _logger.LogInfo(ConfigConstants.IncorrectEmail);
                return false;
            }

            if (string.IsNullOrWhiteSpace(b2cUserId))
            {
                _logger.LogInfo(ConfigConstants.IncorrectUserId);
                return false;
            }

            if (regulatorEmailAllowModel is null || string.IsNullOrWhiteSpace(regulatorEmailAllowModel.KeyValue))
            {
                _logger.LogInfo(ConfigConstants.InvalidEmail);
                return false;
            }

            if (regulatorOrganisationModel is null || string.IsNullOrWhiteSpace(regulatorOrganisationModel.KeyValue))
            {
                _logger.LogInfo(ConfigConstants.InvalidOrganisation);
                return false;
            }
            return true;
        }

        private string CreateNewEnrolment(string organisationName, int nationId, string email, string b2cUserId)
        {
            OrganisationResponseModel regulatorOrganisation = _facadeService
                        .CreateRegulatorOrganisation(organisationName, nationId, ConfigConstants.Regulating)
                        .Result;

            if (regulatorOrganisation is null || string.IsNullOrEmpty(regulatorOrganisation.CreatedOn))
            {
                throw new InvalidDataException(ConfigConstants.FailedOrganisation);
            }

            string newToken = _facadeService.InviteRegulatorUser(
                            new InviteRegulatorRequest
                            {
                                Email = email,
                                RoleKey = ConfigConstants.RoleKey,
                                OrganisationId = regulatorOrganisation.ExternalId,
                                UserId = Guid.Parse(b2cUserId)
                            }).Result;

            return newToken;
        }

        private string RetrieveExistingEnrolment(string email)
        {
            var validEnrolement = CheckExistingEnrolement(email);
            if (validEnrolement.Item1)
            {
                return validEnrolement.Item2;
            }
            else
            {
                _logger.LogInfo(ConfigConstants.FailedRetriveInvite);
                return validEnrolement.Item2;
            }
        }

        private Tuple<bool, string> CheckExistingEnrolement(string email)
        {
            string token = string.Empty;
            try
            {
                string tokenResult = _facadeService.GetValidToken(email).Result.ToString();
                _logger.LogInfo($"Invite token returned {tokenResult}");

                if (!string.IsNullOrWhiteSpace(tokenResult))
                {
                    var jsonToken = JsonSerializer.Deserialize<InviteRegulatorResponseModel>(tokenResult)!;
                    token = jsonToken.InvitedToken.Replace("\"", "");
                }
                return !string.IsNullOrWhiteSpace(token) ? new Tuple<bool, string>(true, token) : new Tuple<bool, string>(false, string.Empty);
            }
            catch
            {
                _logger.LogInfo($"Error returning existing invite token");
                return new Tuple<bool, string>(false, string.Empty);
            }
        }
    }
}