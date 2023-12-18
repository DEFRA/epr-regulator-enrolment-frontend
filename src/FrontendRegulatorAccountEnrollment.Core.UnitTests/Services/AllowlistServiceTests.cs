using FrontendRegulatorAccountEnrollment.Core.Configs;
using FrontendRegulatorAccountEnrollment.Core.Exceptions;
using FrontendRegulatorAccountEnrollment.Core.Models.Responses;
using FrontendRegulatorAccountEnrollment.Core.Services;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace FrontendRegulatorAccountEnrollment.Core.UnitTests.Services
{
    [TestClass]
    public class AllowlistServiceTests
    {
        private Mock<IFacadeService> _facadeServiceMock = null!;
        private Mock<IOptions<RegulatorsDetails>> _regulatorsDetailsOptionsMock = null!;
        private AllowlistService _allowlistService = null!;

        [TestInitialize]
        public void Setup()
        {
            _facadeServiceMock = new Mock<IFacadeService>();
            _regulatorsDetailsOptionsMock = new Mock<IOptions<RegulatorsDetails>>();
            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);
        }

        [TestMethod]
        public void CheckEmailIsAllowListed_WithValidEmail_ReturnsEmailModel()
        {
            // Arrange
            string email = "test@example.com";
            string emailJson = "{\"data\":[{\"KeyValue\":\"test@example.com\"}]}";

            var regulatorsDetails = new RegulatorsDetails
            {
                Emails = emailJson
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act
            var result = _allowlistService.CheckEmailIsAllowListed(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(email, result.KeyValue);
        }

        [TestMethod]
        public void CheckEmailIsAllowListed_WithInvalidEmail_ReturnsNull()
        {
            // Arrange
            string email = "invalid@example.com";
            string emailJson = "{\"data\":[{\"KeyValue\":\"test@example.com\"}]}";

            var regulatorsDetails = new RegulatorsDetails
            {
                Emails = emailJson
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act
            var result = _allowlistService.CheckEmailIsAllowListed(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.KeyName);
            Assert.AreEqual(string.Empty, result.KeyValue);
        }

        [TestMethod]
        public async Task CheckRegulatorOrganisationDetails_WithExistingOrganisation_ReturnsOrganisationData()
        {
            // Arrange
            string regulatorKey = "regulatorKey";
            string organisationJson = "{\"data\":[{\"KeyName\":\"regulatorKey\",\"NationId\":1}]}";

            var regulatorsDetails = new RegulatorsDetails
            {
                Organisations = organisationJson
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation("England"))
                .ReturnsAsync(new OrganisationResponseModel { ExternalId = Guid.NewGuid() });

            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act
            var result =  _allowlistService.QueryRegulatorOrganisationDetails(regulatorKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(regulatorKey, result.KeyName);
            Assert.AreEqual(1, result.NationId);
        }

        [TestMethod]
        public async Task CheckRegulatorOrganisationDetails_WithNonExistingOrganisation_ReturnsOrganisationModel()
        {
            // Arrange
            string regulatorKey = "regulatorKey";
            string organisationJson = "{\"data\":[{\"KeyName\":\"regulatorKey\",\"NationId\":1}]}";

            var regulatorsDetails = new RegulatorsDetails
            {
                Organisations = organisationJson
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _facadeServiceMock.Setup(x => x.CheckOrganisationExistsForNation("1"))
                .ReturnsAsync(new OrganisationResponseModel());

            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act
            var result =  _allowlistService.QueryRegulatorOrganisationDetails(regulatorKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(regulatorKey, result.KeyName);
            Assert.AreEqual(1, result.NationId);
        }

        [TestMethod]
        public async Task CheckRegulatorOrganisationDetails_WithNullOrganisationData_ThrowsArgumentNullException()
        {
            // Arrange
            string regulatorKey = "regulatorKey";

            var regulatorsDetails = new RegulatorsDetails()
            {
                Organisations = string.Empty
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act
            // Assert
             Assert.ThrowsException<JsonException>(() =>
            {
                 _allowlistService.QueryRegulatorOrganisationDetails(regulatorKey);
            });
        }

        [TestMethod]
        public async Task CheckRegulatorOrganisationDetails_WithNonExistingRegulator_ReturnsEmptyClass()
        {
            // Arrange
            string regulatorKey = "regulatorKey";
            string organisationJson = "{\"data\":[{\"KeyName\":\"otherRegulator\",\"NationId\":1}]}";

            var regulatorsDetails = new RegulatorsDetails()
            {
                Organisations = organisationJson
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act

            var result =  _allowlistService.QueryRegulatorOrganisationDetails(regulatorKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.KeyName);
            Assert.AreEqual(0, result.NationId);
        }

        [TestMethod]
        public async Task CheckRegulatorOrganisationDetails_Throws_NoOrganisationAllowListDataException()
        {
            // Arrange
            string regulatorKey = "regulatorKey";
            string organisationJson = "{\"data\":[]}";

            var regulatorsDetails = new RegulatorsDetails()
            {
                Organisations = organisationJson
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act & Assert
            Assert.ThrowsException<NoOrganisationAllowListDataException>(() =>
                _allowlistService.QueryRegulatorOrganisationDetails(regulatorKey));
        }

        [TestMethod]
        public void CheckEmailIsAllowListed_Throws_InvalidDataException()
        {
            // Arrange
            string email = null;
            string emailJson = "{\"data\":[{\"KeyValue\":\"test@example.com\"}]}";

            var regulatorsDetails = new RegulatorsDetails
            {
                Emails = emailJson
            };

            _regulatorsDetailsOptionsMock.Setup(x => x.Value).Returns(regulatorsDetails);
            _allowlistService = new AllowlistService(_facadeServiceMock.Object, _regulatorsDetailsOptionsMock.Object);

            // Act & Assert
            Assert.ThrowsException<InvalidDataException>(() =>
                _allowlistService.CheckEmailIsAllowListed(email));

        }
    }
}
