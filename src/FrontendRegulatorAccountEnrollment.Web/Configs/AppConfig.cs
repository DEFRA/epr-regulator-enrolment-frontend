using System.Diagnostics.CodeAnalysis;

namespace FrontendRegulatorAccountEnrollment.Web.Configs
{
    [ExcludeFromCodeCoverage]
    public class AppConfig
    {
        public const string ConfigSection = "RegulatorPortal";
        public string BaseUrl { get; set; } = string.Empty;
    }
}