using FrontendRegulatorAccountEnrollment.Core.Configs.Models;

namespace FrontendRegulatorAccountEnrollment.Core.Services
{
    public interface IAllowlistService
    {
        EmailModel? CheckEmailIsAllowListed(string email);
        OrganisationModel QueryRegulatorOrganisationDetails(string regulatorKey);
    }
}