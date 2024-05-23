namespace FrontendRegulatorAccountEnrollment.Web.UnitTests
{
    using System.Net;
    using FluentAssertions;
    using FrontendRegulatorAccountEnrollment.Web.Controllers;
    using Microsoft.AspNetCore.Http;

    public class ErrorControllerTests
    {
        private readonly ErrorController _systemUnderTest;
        private readonly Mock<HttpResponse> _mockHttpResponse;

        public ErrorControllerTests()
        {
            var mockHttpContext = new Mock<HttpContext>();
            _mockHttpResponse = new Mock<HttpResponse>();

            _mockHttpResponse.SetupProperty(r => r.StatusCode, (int)HttpStatusCode.OK);
            mockHttpContext.Setup(c => c.Response).Returns(_mockHttpResponse.Object);

            var controllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            _systemUnderTest = new ErrorController
            {
                ControllerContext = controllerContext
            };
        }

        [TestMethod]
        public void Error_ReturnsCorrectResult()
        {
            //Arrange
            var httpContextMock = new Mock<HttpContext>();
            var httpResponse = new Mock<HttpResponse>();
            var errorController = new ErrorController();
            httpContextMock.Setup(x => x.Response).Returns(httpResponse.Object);
            errorController.ControllerContext.HttpContext = httpContextMock.Object;
            //Act
            var result = errorController.Error((int)HttpStatusCode.NotFound);

            //Arrange
            result.Should().BeOfType(typeof(ViewResult));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow((int)HttpStatusCode.NotFound)]
        [DataRow((int)HttpStatusCode.InternalServerError)]
        [DataRow((int)HttpStatusCode.BadRequest)]
        public void Error_ReturnsErrorView_WhenCalledWithErrorCode(int? statusCode)
        {
            // Arrange
            string expectedPageName = "Error";

            // Act
            var viewResult = _systemUnderTest.Error(statusCode);

            // Assert
            viewResult.ViewName.Should().Be(expectedPageName);

            if (statusCode == null)
            {
                _systemUnderTest.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            }
            else
            {
                _systemUnderTest.Response.StatusCode.Should().Be(statusCode);

            }
        }
    }
}
