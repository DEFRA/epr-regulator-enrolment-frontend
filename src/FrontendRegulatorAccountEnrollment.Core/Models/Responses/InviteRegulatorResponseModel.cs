using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontendRegulatorAccountEnrollment.Core.Models.Responses
{
    [ExcludeFromCodeCoverage]
    public class InviteRegulatorResponseModel
    {
        [JsonPropertyName("invitedToken")]
        public string InvitedToken { get; set; } = string.Empty;
    }
}
