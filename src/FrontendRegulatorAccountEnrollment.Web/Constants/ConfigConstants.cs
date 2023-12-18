using System.Diagnostics.CodeAnalysis;
using Microsoft.Identity.Web;

namespace FrontendRegulatorAccountEnrollment.Web.Constants
{
    [ExcludeFromCodeCoverage]
    public static class ConfigConstants
    {
        public const string Emails = "emails";
        public const string IncorrectEmail = "Claims Identity email is null or incorrect";
        public const string IncorrectUserId = $"Claims Identity does not {ClaimConstants.ObjectId} contain or incorrect";
        public const string InvalidEmail = "User email is not on Allowlist";
        public const string InvalidOrganisation = "User organisation is not on Allowlist";
        public const string FailedOrganisation = "Failed to create Regulator Organisation entry in the DB";
        public const string OrganisationExistsWithoutEnrolement = "The Organisation has been created but this user has no Enrolement, not an abandoned journey.";
        public const string FailedRetrive = "Failed to retrive or generate a valid invite token";
        public const string FailedRetriveInvite = "Failed to retrive a valid invite token";
        public const string Regulating = "Regulating";
        public const string RoleKey = "Regulator.Admin";
    }
}
