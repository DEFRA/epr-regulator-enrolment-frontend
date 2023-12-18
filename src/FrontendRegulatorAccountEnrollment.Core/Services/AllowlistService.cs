using FrontendRegulatorAccountEnrollment.Core.Configs;
using FrontendRegulatorAccountEnrollment.Core.Configs.Models;
using FrontendRegulatorAccountEnrollment.Core.Exceptions;
using Microsoft.Extensions.Options;

using System.Globalization;
using System.Text.Json;

namespace FrontendRegulatorAccountEnrollment.Core.Services
{
    public class AllowlistService : IAllowlistService
    {
        private readonly RegulatorsDetails _regulatorsDetails;

        public AllowlistService(IFacadeService facadeService,
            IOptions<RegulatorsDetails> regulatorDetails)
        {
            _regulatorsDetails = regulatorDetails.Value;
        }

        public EmailModel CheckEmailIsAllowListed(string email)
        {
            if (email == null)
            {
                throw new InvalidDataException(nameof(email));
            }

            var emailData = JsonSerializer.Deserialize<EmailJson>(_regulatorsDetails.Emails);
            var result = emailData?.Data.FirstOrDefault(x => x.KeyValue.ToString().Trim().ToLower(CultureInfo.CurrentCulture) == email);

            return emailData is null || result is null ? new EmailModel() : result;
        }

        public OrganisationModel QueryRegulatorOrganisationDetails(string regulatorKey)
        {
            var organisationAllowlistData = JsonSerializer
                .Deserialize<OrganisationJson>(_regulatorsDetails.Organisations);

            if (organisationAllowlistData == null || organisationAllowlistData.Data.Count == 0)
            {
                throw new NoOrganisationAllowListDataException(nameof(organisationAllowlistData));
            }

            OrganisationModel? regulatorOrganisation = organisationAllowlistData
                .Data
                .FirstOrDefault(x => x.KeyName.Equals(regulatorKey));

            if (regulatorOrganisation is not null)
            {
                return regulatorOrganisation;
            }
            return new OrganisationModel();
        }
    }
}