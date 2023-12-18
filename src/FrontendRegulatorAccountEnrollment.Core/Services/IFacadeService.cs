using FrontendRegulatorAccountEnrollment.Core.Models.Requests;
using FrontendRegulatorAccountEnrollment.Core.Models.Responses;

namespace FrontendRegulatorAccountEnrollment.Core.Services
{
    public interface IFacadeService
    {
        Task<OrganisationResponseModel> CheckOrganisationExistsForNation(string nationName);
        
        Task<OrganisationResponseModel> CreateRegulatorOrganisation(
            string organisationName, int nationId, string serviceId);
        
        Task<string> InviteRegulatorUser(InviteRegulatorRequest inviteRegulatorRequest);

        Task<string> GetValidToken(string email);
    }
}