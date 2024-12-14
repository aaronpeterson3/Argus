using Xunit;
using Orleans.TestingHost;
using Moq;
using Argus.Features.Tenants.Grains;
using Argus.Features.Authentication.Domain.Services;

namespace Argus.Features.Tenants.Tests.Grains
{
    public class TenantGrainTests
    {
        private readonly TestCluster _cluster;
        private readonly Mock<IEmailService> _emailService;

        public TenantGrainTests()
        {
            var builder = new TestClusterBuilder();
            _emailService = new Mock<IEmailService>();
            builder.AddService(_emailService.Object);
            _cluster = builder.Build();
            _cluster.Deploy();
        }

        [Fact]
        public async Task CreateAsync_WithValidData_CreatesTenant()
        {
            // Arrange
            var grain = _cluster.GrainFactory.GetGrain<ITenantGrain>("tenant1");

            // Act
            var result = await grain.CreateAsync("Test Tenant", "owner123");
            var state = await grain.GetStateAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(state);
            Assert.Equal("Test Tenant", state.Name);
            Assert.Equal("owner123", state.OwnerId);
            Assert.Single(state.Users);
        }

        [Fact]
        public async Task InviteUserAsync_SendsEmail()
        {
            // Arrange
            var grain = _cluster.GrainFactory.GetGrain<ITenantGrain>("tenant1");
            await grain.CreateAsync("Test Tenant", "owner123");

            // Act
            var result = await grain.InviteUserAsync("test@example.com", "User");

            // Assert
            Assert.True(result.IsSuccess);
            _emailService.Verify(x => x.SendTenantInviteEmailAsync("test@example.com", "Test Tenant", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AcceptInviteAsync_WithValidToken_AddsUser()
        {
            // Arrange
            var grain = _cluster.GrainFactory.GetGrain<ITenantGrain>("tenant1");
            await grain.CreateAsync("Test Tenant", "owner123");
            var inviteResult = await grain.InviteUserAsync("test@example.com", "User");

            // Act
            var result = await grain.AcceptInviteAsync(inviteResult.Value, "user123");
            var state = await grain.GetStateAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, state.Users.Count);
            Assert.Contains(state.Users, u => u.UserId == "user123");
        }

        public void Dispose()
        {
            _cluster?.StopAllSilos();
        }
    }
}