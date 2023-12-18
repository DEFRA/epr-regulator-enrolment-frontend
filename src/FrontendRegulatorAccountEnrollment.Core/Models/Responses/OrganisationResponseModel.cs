using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontendRegulatorAccountEnrollment.Core.Models.Responses
{
    [ExcludeFromCodeCoverage]
    public class OrganisationResponseModel
    {
        [JsonPropertyName("createdOn")]
        public string CreatedOn { get; set; } = string.Empty;

        [JsonPropertyName("externalId")]
        public Guid ExternalId { get; set; }
    }
}