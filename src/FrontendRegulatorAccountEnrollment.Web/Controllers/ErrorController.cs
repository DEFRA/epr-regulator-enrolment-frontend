namespace FrontendRegulatorAccountEnrollment.Web.Controllers
{
    using FrontendRegulatorAccountEnrollment.Web.Constants;
    using System.Net;

    using Microsoft.AspNetCore.Mvc;

    public class ErrorController : Controller
    {
        [Route(PagePath.ErrorUrl)]
        public ViewResult Error(int? statusCode)
        {
            Response.StatusCode = statusCode ?? (int)HttpStatusCode.InternalServerError;

            return View("Error");
        }
    }
}
