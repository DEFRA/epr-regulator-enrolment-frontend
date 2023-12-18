namespace FrontendRegulatorAccountEnrollment.Web.ViewComponents
{
    using FrontendRegulatorAccountEnrollment.Web.Configs;
    using FrontendRegulatorAccountEnrollment.Web.Models;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.Options;

    public class PhaseBannerViewComponent : ViewComponent
    {
        private readonly PhaseBannerOptions _bannerOptions;

        public PhaseBannerViewComponent(IOptions<PhaseBannerOptions> bannerOptions)
        {
            _bannerOptions = bannerOptions.Value;
        }

        public ViewViewComponentResult Invoke()
        {
            var phaseBannerModel = new PhaseBannerViewModel()
            {
                Status = _bannerOptions!.ApplicationStatus,
                Url = _bannerOptions!.SurveyUrl,
                ShowBanner = _bannerOptions!.Enabled
            };
            return View(phaseBannerModel);
        }
    }
}
