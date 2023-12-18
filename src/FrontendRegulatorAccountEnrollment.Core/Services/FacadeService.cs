using FrontendRegulatorAccountEnrollment.Core.Configs;
using FrontendRegulatorAccountEnrollment.Core.Models.Requests;
using FrontendRegulatorAccountEnrollment.Core.Models.Responses;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace FrontendRegulatorAccountEnrollment.Core.Services
{
    public class FacadeService : IFacadeService 
    {
        private const string CheckRegulatorOrganisationExistsPath = "CheckRegulatorOrganisationExistsPath";
        private const string CreateRegulatorOrganisationPath = "CreateRegulatorOrganisationPath";
        private const string InviteRegulatorUserPath = "InviteRegulatorUserPath";
        private readonly string[] _scopes;
        private readonly HttpClient _httpClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly FacadeApiConfig _facadeApiConfig;
        private const string GetExistingTokenPath = "GetExistingTokenPath";

        public FacadeService(
            HttpClient httpClient,
            ITokenAcquisition tokenAcquisition,
            IOptions<FacadeApiConfig> facadeApiConfig)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _facadeApiConfig = facadeApiConfig.Value;
            _scopes = new[]
            {
                _facadeApiConfig.DownstreamScope
            };
        }

        public async Task<OrganisationResponseModel> CheckOrganisationExistsForNation(string nationName)
        {
            await PrepareAuthenticatedClient();

            string path = _facadeApiConfig.Endpoints[CheckRegulatorOrganisationExistsPath]
                .Replace("{0}", nationName);

            var response = await _httpClient.GetAsync(path);

            string result = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(result))
            {
                return JsonSerializer.Deserialize<OrganisationResponseModel>(result)!;
            }

            return new OrganisationResponseModel();
        }

        public async Task<OrganisationResponseModel> CreateRegulatorOrganisation(
            string organisationName, int nationId, string serviceId)
        {
            await PrepareAuthenticatedClient();

            string json = JsonSerializer.Serialize(new
            {
                name = organisationName,
                nationId,
                serviceId
            });

            string path = _facadeApiConfig.Endpoints[CreateRegulatorOrganisationPath];

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(path, stringContent);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrganisationResponseModel>(result)!;
            }

            return new OrganisationResponseModel();
        }

        public async Task<string> InviteRegulatorUser(InviteRegulatorRequest inviteRegulatorRequest)
        {
            await PrepareAuthenticatedClient();

            string json = JsonSerializer.Serialize(inviteRegulatorRequest);
            string path = _facadeApiConfig.Endpoints[InviteRegulatorUserPath];

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(path, stringContent);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();                
                var jsonResult = JsonSerializer.Deserialize<InviteRegulatorResponseModel>(result)!;
                string formattedResult = jsonResult.InvitedToken.Replace("\"", "");

                return formattedResult;
            }

            return string.Empty;
        }

        public async Task<string> GetValidToken(string email)
        {
            await PrepareAuthenticatedClient();

            string path = _facadeApiConfig.Endpoints[GetExistingTokenPath]
                .Replace("{0}", email);

            var response = await _httpClient.GetAsync(path);

            string result = await response.Content.ReadAsStringAsync();
            
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            return string.Empty;
        }

        private async Task PrepareAuthenticatedClient()
        {
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_facadeApiConfig.BaseUrl);

                string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(Microsoft.Identity.Web.Constants.Bearer, accessToken);
            }
        }      
    }
}