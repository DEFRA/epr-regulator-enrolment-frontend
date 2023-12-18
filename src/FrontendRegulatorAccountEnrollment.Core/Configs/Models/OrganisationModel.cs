using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontendRegulatorAccountEnrollment.Core.Configs.Models
{
    [ExcludeFromCodeCoverage]
    public class OrganisationJson
    {
        [JsonPropertyName("data")]
        public List<OrganisationModel> Data { get; set; } = new List<OrganisationModel>();
    }

    [ExcludeFromCodeCoverage]
    public class OrganisationModel
    {
        [JsonPropertyName("KeyName")]
        public string KeyName { get; set; } = string.Empty;

        [JsonPropertyName("KeyValue")]
        public string KeyValue { get; set; } = string.Empty;

        [JsonPropertyName("OrganisationTypeId")]
        public int OrganisationTypeId { get; set; }

        [JsonPropertyName("NationId")]
        public int NationId { get; set; }
    }
}