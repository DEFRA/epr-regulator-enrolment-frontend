using System.Diagnostics.CodeAnalysis;
using FrontendRegulatorAccountEnrollment.Core.Models.Requests;
using FrontendRegulatorAccountEnrollment.Core.Models.Responses;

namespace FrontendRegulatorAccountEnrollment.Core.Services
{
    [ExcludeFromCodeCoverage]
    public class MockedFacadeService : IFacadeService
    {
        public async Task<OrganisationResponseModel> CheckOrganisationExistsForNation(string nationName) =>
            await Task.FromResult(new OrganisationResponseModel
            {
                CreatedOn = "2023-07-07 08:22",
                ExternalId = Guid.Parse("46dda877-4729-4120-b93c-35452953b619")
            });

        public async Task<OrganisationResponseModel> CreateRegulatorOrganisation(
            string organisationName, int nationId, string serviceId) =>
            await Task.FromResult(new OrganisationResponseModel
            {
                CreatedOn = "2023-07-07 08:22",
                ExternalId = Guid.Parse("46dda877-4729-4120-b93c-35452953b619")
            });

        public async Task<string> InviteRegulatorUser(InviteRegulatorRequest inviteRegulatorRequest) =>
            await Task.FromResult(Guid.NewGuid().ToString());

        public async Task<string> GetValidToken(string email) =>
            await Task.FromResult(Guid.NewGuid().ToString());
    }
}