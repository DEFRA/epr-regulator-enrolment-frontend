using System.Diagnostics.CodeAnalysis;

namespace FrontendRegulatorAccountEnrollment.Core.Configs
{
    [ExcludeFromCodeCoverage]
    public class RegulatorsDetails
    {
        public const string ConfigSection = "Regulators";

        public string Emails { get; set; } = string.Empty;
        
        public string Organisations { get; set; } = string.Empty;
    }
}