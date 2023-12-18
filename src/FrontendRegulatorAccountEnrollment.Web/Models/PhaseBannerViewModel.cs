using System.Diagnostics.CodeAnalysis;

namespace FrontendRegulatorAccountEnrollment.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class PhaseBannerViewModel
    {
        public string Status { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public bool ShowBanner { get; set; }
    }
}