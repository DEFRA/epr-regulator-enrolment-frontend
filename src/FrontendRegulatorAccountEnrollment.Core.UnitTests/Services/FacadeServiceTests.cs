using FrontendRegulatorAccountEnrollment.Core.Configs;
using FrontendRegulatorAccountEnrollment.Core.Models.Requests;
using FrontendRegulatorAccountEnrollment.Core.Models.Responses;
using FrontendRegulatorAccountEnrollment.Core.Services;
using FrontendRegulatorAccountEnrollment.Web.Constants;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace FrontendRegulatorAccountEnrollment.Core.UnitTests.Services
{
    [TestClass]
    public class FacadeServiceTests
    {
        private Mock<HttpMessageHandler> _mockHandler = null!;
        private Mock<ITokenAcquisition> _tokenAcquisitionMock = null!;
        private HttpClient _httpClient = null!;
        private FacadeApiConfig _facadeApiConfig = null!;
        private FacadeService _facadeService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            _httpClient = new HttpClient(_mockHandler.Object);
            _httpClient.BaseAddress = new Uri("http://example");
            _facadeApiConfig = new FacadeApiConfig
            {
                BaseUrl = "https://example.com/api/",
                Endpoints = new()
                {
                    { "CheckRegulatorOrganisationExistsPath", "check-organisation/{0}" },
                    { "CreateRegulatorOrganisationPath", "create-organisation" },
                    { "InviteRegulatorUserPath", "invite-user" },
                    { "GetExistingTokenPath", "accounts-management/invited-regulator-user?email={0}" }
                },
                DownstreamScope = "api_scope"
            };
            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, Options.Create(_facadeApiConfig));
        }

        [TestMethod]
        public async Task CheckOrganisationExistsForNation_ShouldReturnOrganisationResponseModel_WhenRequestIsSuccessful()
        {
            // Arrange
            string nationName = "TestNation";

            var expectedResponse = new OrganisationResponseModel
            {
                CreatedOn = "2023-07-07 08:22",
                ExternalId = Guid.Parse("46dda877-4729-4120-b93c-35452953b619")
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.CheckOrganisationExistsForNation(nationName);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse.CreatedOn, response?.CreatedOn);
            Assert.AreEqual(expectedResponse.ExternalId, response?.ExternalId);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.AbsolutePath.Contains("check-organisation/TestNation")
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task CheckOrganisationExistsForNation_ShouldReturnNull_WhenResponseIsEmpty()
        {
            // Arrange
            string nationName = "TestNation";
            OrganisationResponseModel? expectedResponse = null;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.CheckOrganisationExistsForNation(nationName);

            // Assert
            Assert.IsNull(response);
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.AbsolutePath.Contains("check-organisation/TestNation")
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task CreateRegulatorOrganisation_ShouldReturnOrganisationResponseModel_WhenRequestIsSuccessful()
        {
            // Arrange
            string organisationName = "TestOrg";
            int nationId = 123;
            string serviceId = "service123";

            var expectedResponse = new OrganisationResponseModel
            {
                CreatedOn = "2023-07-07 08:22",
                ExternalId = Guid.Parse("46dda877-4729-4120-b93c-35452953b619")
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.CreateRegulatorOrganisation(organisationName, nationId, serviceId);
            string serializedRequest = JsonSerializer.Serialize(new { name = organisationName, nationId, serviceId });

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse.CreatedOn, response.CreatedOn);
            Assert.AreEqual(expectedResponse.ExternalId, response.ExternalId);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.AbsolutePath.Contains("/create-organisation") &&
                    GetRequestBodyContent(req) == serializedRequest
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task CreateRegulatorOrganisation_ShouldReturnNull_WhenRequestIsUnsuccessful()
        {
            // Arrange
            string organisationName = "TestOrg";
            int nationId = 123;
            string serviceId = "service123";

            var expectedResponse = new OrganisationResponseModel
            {
                CreatedOn = string.Empty,
                ExternalId = Guid.Empty,
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.CreateRegulatorOrganisation(organisationName, nationId, serviceId);
            string serializedRequest = JsonSerializer.Serialize(new { name = organisationName, nationId, serviceId });

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse.CreatedOn, response.CreatedOn);
            Assert.AreEqual(expectedResponse.ExternalId, response.ExternalId);
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.AbsolutePath.Contains("/create-organisation") &&
                    GetRequestBodyContent(req) == serializedRequest
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task InviteRegulatorUser_ShouldReturnStringResponse_WhenRequestIsSuccessful()
        {
            // Arrange
            var request = new InviteRegulatorRequest
            {
                Email = "test@test.com",
                RoleKey = ConfigConstants.RoleKey,
                OrganisationId = Guid.Parse("46dda877-4729-4120-b93c-35452953b619")
            };

            var expectedResponse = new InviteRegulatorResponseModel
            {
                InvitedToken = Guid.NewGuid().ToString()
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            string response = await _facadeService.InviteRegulatorUser(request);
            string serializedRequest = JsonSerializer.Serialize(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse.InvitedToken, response);
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.AbsolutePath.Contains("/invite-user") &&
                    GetRequestBodyContent(req) == serializedRequest
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task InviteRegulatorUser_ShouldReturnEmptyStringResponse_WhenRequestIsUnsuccessful()
        {
            // Arrange
            var request = new InviteRegulatorRequest
            {
                Email = "test@test.com",
                RoleKey = ConfigConstants.RoleKey,
                OrganisationId = Guid.Parse("46dda877-4729-4120-b93c-35452953b619")
            };

            string expectedResponse = string.Empty;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            string response = await _facadeService.InviteRegulatorUser(request);
            string serializedRequest = JsonSerializer.Serialize(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse, response);
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.AbsolutePath.Contains("/invite-user") &&
                    GetRequestBodyContent(req) == serializedRequest
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetValidToken_ShouldReturnStringResponse_WhenRequestIsSuccessful()
        {
            // Arrange
            string validEmail = "valid.test@test.com";
            string expectedResponse = Guid.NewGuid().ToString();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            string result = await _facadeService.GetValidToken(validEmail);
            string response = result.Replace("\"", "");

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse, response);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task IGetValidToken_ShouldReturnEmptyStringResponse_WhenRequestIsUnsuccessful()
        {
            // Arrange
            string invalidEmail = "invalid.test@test.com";
            string expectedResponse = string.Empty;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            string result = await _facadeService.GetValidToken(invalidEmail);
            string response = result.Replace("\"", "");

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse, response);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task CheckOrganisationExistsForNation_ShouldReturnNull_WhenRequestIsNotSuccessful()
        {
            // Arrange
            string nationName = "TestNation";

            var expectedResponse = new OrganisationResponseModel();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = null,
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var response = await _facadeService.CheckOrganisationExistsForNation(nationName);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse.CreatedOn, response?.CreatedOn);
            Assert.AreEqual(expectedResponse.ExternalId, response?.ExternalId);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.AbsolutePath.Contains("check-organisation/TestNation")
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetValidToken_ShouldReturnEmptyStringResponse_WhenRequestIsSuccessful()
        {
            // Arrange
            string validEmail = "valid.test@test.com";
            string expectedResponse = string.Empty;

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            string result = await _facadeService.GetValidToken(validEmail);
            string response = result.Replace("\"", "");

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedResponse, response);

            httpTestHandler.Dispose();
        }

        private static string? GetRequestBodyContent(HttpRequestMessage request) =>
            request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}