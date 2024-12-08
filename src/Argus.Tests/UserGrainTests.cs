using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestKit;
using Argus.Abstractions;
using Argus.Grains;

namespace Argus.Tests
{
    /// <summary>
    /// Tests for the UserGrain implementation
    /// </summary>
    [TestClass]
    public class UserGrainTests
    {
        private TestKitSilo silo;

        /// <summary>
        /// Initializes test context
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            silo = new TestKitSilo();
        }

        /// <summary>
        /// Tests that creating a user with valid data succeeds
        /// </summary>
        [TestMethod]
        public async Task CreateUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            var grain = silo.CreateGrain<UserGrain>("test@example.com");
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

        /// <summary>
        /// Tests that validating correct credentials succeeds
        /// </summary>
        [TestMethod]
        public async Task ValidateCredentials_WithCorrectPassword_ShouldSucceed()
        {
            // Arrange
            var grain = silo.CreateGrain<UserGrain>("test@example.com");
            var profile = new UserProfile
            {
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "JohnD"
            };
            await grain.CreateAsync("test@example.com", "Password123!", profile);

            // Act
            var result = await grain.ValidateCredentialsAsync("Password123!");

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that validating incorrect credentials fails
        /// </summary>
        [TestMethod]
        public async Task ValidateCredentials_WithIncorrectPassword_ShouldFail()
        {
            // Arrange
            var grain = silo.CreateGrain<UserGrain>("test@example.com");
            var profile = new UserProfile
            {
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "JohnD"
            };
            await grain.CreateAsync("test@example.com", "Password123!", profile);

            // Act
            var result = await grain.ValidateCredentialsAsync("WrongPassword!");

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Cleans up test resources
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            silo?.Dispose();
        }
    }
}