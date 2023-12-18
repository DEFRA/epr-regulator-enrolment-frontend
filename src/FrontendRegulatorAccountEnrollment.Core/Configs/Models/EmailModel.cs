using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FrontendRegulatorAccountEnrollment.Core.Configs.Models
{
    [ExcludeFromCodeCoverage]
    public class EmailJson
    {
        [JsonPropertyName("data")]
        public List<EmailModel> Data { get; set; } = new List<EmailModel>();
    }
    
    public class EmailModel
    {
        [JsonPropertyName("KeyName")]
        public string KeyName { get; set; } = string.Empty;

        [JsonPropertyName("KeyValue")]
        public string KeyValue { get; set; } = string.Empty;
    }
}