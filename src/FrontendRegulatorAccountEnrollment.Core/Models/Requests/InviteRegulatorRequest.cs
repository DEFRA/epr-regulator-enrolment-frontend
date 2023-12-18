using System.Diagnostics.CodeAnalysis;

namespace FrontendRegulatorAccountEnrollment.Core.Models.Requests
{
    [ExcludeFromCodeCoverage]
    public class InviteRegulatorRequest
    {
        public string Email { get; set; } = null!;
        public string RoleKey { get; set; } = null!;
        public Guid OrganisationId { get; set; }
        public Guid UserId { get; set; }
    }
}