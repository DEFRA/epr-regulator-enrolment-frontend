using FrontendRegulatorAccountEnrollment.Core.Configs.Models;
using FrontendRegulatorAccountEnrollment.Core.Models.Requests;
using FrontendRegulatorAccountEnrollment.Core.Models.Responses;
using FrontendRegulatorAccountEnrollment.Core.Services;
using FrontendRegulatorAccountEnrollment.Web.Configs;
using FrontendRegulatorAccountEnrollment.Web.Constants;
using FrontendRegulatorAccountEnrollment.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Globalization;
using System.Security.Claims;

namespace FrontendRegulatorAccountEnrollment.Web.UnitTests
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<ILogger<HomeController>> _loggerMock = null!;
        private Mock<IFacadeService> _facadeServiceMock = null!;
        private Mock<IAllowlistService> _allowlistServiceMock = null!;
        private HomeController _homeController = null!;
        private Mock<IOptions<AppConfig>> _configurationOptionsMock = null!;
        private const string Email = "test@example.com";
        private const string BaseUrl = "https://dummytest/";
        private const string TestingTokenValue = "testingToken";
        private const string TestingToken = "{\"invitedToken\":\"testingToken\"}";
        private const string RedirectUrl = $"{BaseUrl}?token={TestingTokenValue}";
        private const string ErrorRedirectUrl = $"~/error";

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _facadeServiceMock = new Mock<IFacadeService>();
            _allowlistServiceMock = new Mock<IAllowlistService>();
            _configurationOptionsMock = new Mock<IOptions<AppConfig>>();
            _configurationOptionsMock
                .Setup(x => x.Value)
                .Returns(new AppConfig
                {
                    BaseUrl = BaseUrl
                });
            _homeController = new HomeController(
                _loggerMock.Object,
                _facadeServiceMock.Object,
                _allowlistServiceMock.Object,
                _configurationOptionsMock.Object
            );
        }

        [TestMethod]
        public void InviteUser_WithAllowlistedEmailAndSuccessfulOrganisationCreation_ReturnsOkResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyName = "EA_ORG", KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "Org1", NationId = 1 };
            var organisationResponseModel = new OrganisationResponseModel {CreatedOn = DateTimeOffset.Now.ToString(CultureInfo.CurrentCulture), ExternalId = Guid.NewGuid() };
            var checkOrganisationExistResponseModel = new OrganisationResponseModel { ExternalId = Guid.Empty };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);

            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(It.IsAny<string>()))
                .Returns(organisationModel);

            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation(It.IsAny<string>()))
                .ReturnsAsync(checkOrganisationExistResponseModel);

            _facadeServiceMock.Setup(x => x.CreateRegulatorOrganisation(
                organisationModel.KeyValue, organisationModel.NationId, ConfigConstants.Regulating))
                .ReturnsAsync(organisationResponseModel);

            _facadeServiceMock.Setup(x => x.InviteRegulatorUser(It.IsAny<InviteRegulatorRequest>()))
                .ReturnsAsync(TestingTokenValue);

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(RedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithInvalidEmail_ReturnsErrorRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var organisationModel = new OrganisationModel { KeyValue = "Org1", NationId = 1 };

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns((EmailModel?)null);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(Email))
                .Throws(new Exception("Some exception"));

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithOrganisationCreationFailure_ReturnsErrorRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "Org1", NationId = 1 };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(emailModel.KeyValue))
                .Returns(organisationModel);

            _facadeServiceMock.Setup(x => x.CreateRegulatorOrganisation(
                organisationModel.KeyValue, organisationModel.NationId, ConfigConstants.Regulating))
                .Throws(new Exception("Some exception"));

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithException_ReturnsErrorRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyValue = Email };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(emailModel.KeyValue))
                .Throws(new Exception("Some exception"));

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void Error_ReturnsViewResult()
        {
            // Act
            var result = _homeController.Error();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void InviteUser_WithAbandonedJourney_ReturnsOkResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyName = "EA_ORG", KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "England", NationId = 1 };
            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(emailModel.KeyName))
                .Returns(organisationModel);

            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation(
                organisationModel.KeyValue))
                .ReturnsAsync(organisationResponseModel);
            _facadeServiceMock.Setup(x => x.GetValidToken(It.IsAny<string>())).Returns(Task.FromResult(TestingToken));

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(RedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithoutAbandonedJourney_ReturnsErrorRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyName = "EA_ORG", KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "England", NationId = 1 };
            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(It.IsAny<string>()))
                .Returns(organisationModel);
            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation(
                organisationModel.KeyValue))
                .ReturnsAsync(organisationResponseModel);
            _facadeServiceMock.Setup(x => x.GetValidToken(It.IsAny<string>()))
                 .Throws(new Exception("Some exception"));

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }



        [TestMethod]
        public void InviteUser_WithOrganisationCreation_ReturnsRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var organisationResponseModel = new OrganisationResponseModel { CreatedOn = DateTimeOffset.Now.ToString(CultureInfo.CurrentCulture), ExternalId = Guid.NewGuid() };
            var checkOrganisationExistsResponseModel = new OrganisationResponseModel { ExternalId = Guid.Empty };

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyName = "EA_ORG", KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "Org1", NationId = 1 };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);

            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(emailModel.KeyName))
                .Returns(organisationModel);

            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation("This Test Nation")).ReturnsAsync(organisationResponseModel);

            _facadeServiceMock.Setup(x => x.CreateRegulatorOrganisation(
                organisationModel.KeyValue, organisationModel.NationId, ConfigConstants.Regulating))
                .ReturnsAsync(organisationResponseModel);

            _facadeServiceMock.Setup(x => x.InviteRegulatorUser(It.IsAny<InviteRegulatorRequest>()))
                .ReturnsAsync(TestingTokenValue);

            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation(It.IsAny<string>()))
                .ReturnsAsync(checkOrganisationExistsResponseModel);

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(RedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithInvalidB2CEmail_ReturnsErrorRedirect()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, string.Empty),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithInvalidB2CUserId_ReturnsErrorRedirect()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, string.Empty)
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithInvalidEmailModel_ReturnsErrorRedirect()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel();
            var organisationModel = new OrganisationModel { KeyValue = "Org1", NationId = 1 };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithOrganisationCreationReturnsNull_ReturnsErrorRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyName = "EA_ORG", KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "Org1", NationId = 1 };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(emailModel.KeyName))
                .Returns(organisationModel);
            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation("This Test Nation")).ReturnsAsync(organisationResponseModel);
            _facadeServiceMock.Setup(x => x.CreateRegulatorOrganisation(
                organisationModel.KeyValue, organisationModel.NationId, ConfigConstants.Regulating))
                .ReturnsAsync(new OrganisationResponseModel());
            _facadeServiceMock.Setup(x => x.InviteRegulatorUser(It.IsAny<InviteRegulatorRequest>()))
                .ReturnsAsync(TestingTokenValue);

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithAbandonedFailedToRetriveJourney_ReturnsErrorRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyName = "EA_ORG", KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "England", NationId = 1 };
            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(It.IsAny<string>()))
                .Returns(organisationModel);
            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation(
                organisationModel.KeyValue))
                .ReturnsAsync(organisationResponseModel);
            _facadeServiceMock.Setup(x => x.GetValidToken(It.IsAny<string>()))
                 .Throws(new Exception("Some exception"));

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }

        [TestMethod]
        public void InviteUser_WithAbandonedRetriveEmptyToken_ReturnsErrorRedirectResult()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, Email),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString())
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var emailModel = new EmailModel { KeyName = "EA_ORG", KeyValue = Email };
            var organisationModel = new OrganisationModel { KeyValue = "England", NationId = 1 };
            var organisationResponseModel = new OrganisationResponseModel { ExternalId = Guid.NewGuid() };

            _allowlistServiceMock.Setup(x => x.CheckEmailIsAllowListed(Email)).Returns(emailModel);
            _allowlistServiceMock.Setup(x => x.QueryRegulatorOrganisationDetails(It.IsAny<string>()))
                .Returns(organisationModel);
            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation(
                organisationModel.KeyValue))
                .ReturnsAsync(organisationResponseModel);
            _facadeServiceMock.Setup(x => x.GetValidToken(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            // Act
            var result = _homeController.InviteUser();

            // Assert
            Assert.IsInstanceOfType(result, typeof(Task<RedirectResult>));
            Assert.AreEqual(false, result.IsFaulted);
            Assert.AreEqual(ErrorRedirectUrl, result.Result.Url);
        }
    }
}