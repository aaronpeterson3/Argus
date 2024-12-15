using Xunit;
using Orleans.TestingHost;
using Argus.Features.Users.Grains;
using Argus.Features.Users.Domain.Models;

namespace Argus.Features.Users.Tests.Grains
{
    public class UserGrainTests
    {
        private readonly TestCluster _cluster;

        public UserGrainTests()
        {
            var builder = new TestClusterBuilder();
            _cluster = builder.Build();
            _cluster.Deploy();
        }

        [Fact]
        public async Task CreateAsync_WithValidData_CreatesUser()
        {
            // Arrange
            var grain = _cluster.GrainFactory.GetGrain<IUserGrain>("test@example.com");
            var profile = new UserProfile
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };

            // Act
            var result = await grain.CreateAsync("test@example.com", "password123", profile);
            var state = await grain.GetStateAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(state);
            Assert.Equal("test@example.com", state.Email);
            Assert.Equal("Test", state.Profile.FirstName);
        }

        [Fact]
        public async Task UpdateProfileAsync_WithValidData_UpdatesProfile()
        {
            // Arrange
            var grain = _cluster.GrainFactory.GetGrain<IUserGrain>("test@example.com");
            var initialProfile = new UserProfile
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };
            await grain.CreateAsync("test@example.com", "password123", initialProfile);

            // Act
            var updatedProfile = new UserProfile
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = "test@example.com"
            };
            var result = await grain.UpdateProfileAsync(updatedProfile);
            var state = await grain.GetStateAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Updated", state.Profile.FirstName);
            Assert.Equal("Name", state.Profile.LastName);
        }

        public void Dispose()
        {
            _cluster?.StopAllSilos();
        }
    }
}