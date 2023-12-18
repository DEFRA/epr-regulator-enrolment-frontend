using System.Diagnostics.CodeAnalysis;

namespace FrontendRegulatorAccountEnrollment.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class ErrorViewModel
	{
		public string? RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}