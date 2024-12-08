using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using Argus.Abstractions;
using Moq;

namespace Argus.Tests
{
    [TestClass]
    public class UserGrainTests
    {
        private TestCluster _cluster;

        [TestInitialize]
        public void Initialize()
        {
            var builder = new TestClusterBuilder();
            _cluster = builder.Build();
            _cluster.Deploy();
        }

        [TestMethod]
        public async Task CreateUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            var grain = _cluster.GrainFactory.GetGrain<IUserGrain>("test@example.com");
            var profile = new UserProfile
            {
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "JohnD"
            };

            // Act
            var result = await grain.CreateAsync("test@example.com", "Password123!", profile);
            var state = await grain.GetStateAsync();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("test@example.com", state.Email);
            Assert.AreEqual("John", state.Profile.FirstName);
            Assert.AreEqual("Doe", state.Profile.LastName);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cluster?.StopAllSilos();
        }
    }
}