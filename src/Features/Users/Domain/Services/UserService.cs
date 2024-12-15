using Argus.Core.Infrastructure.Events;
using Argus.Core.Infrastructure.BackgroundJobs;
using Argus.Core.Infrastructure.Caching;
using Argus.Features.Users.Infrastructure.Events;
using Argus.Features.Users.Infrastructure.Jobs;

namespace Argus.Features.Users.Domain.Services
{
    public interface IUserService
    {
        Task<Result<string>> CreateUserAsync(string email, string password, UserProfile profile);
        Task<Result<UserProfile>> GetProfileAsync(string userId);
        Task<Result<Unit>> UpdateProfileAsync(string userId, UserProfile profile);
        Task<Result<string>> RequestDataExportAsync(string userId, string outputFormat, string notificationEmail);
    }

    public class UserService : IUserService
    {
        private readonly IClusterClient _clusterClient;
        private readonly IEventPublisher _eventPublisher;
        private readonly IJobService _jobService;
        private readonly ICacheService _cacheService;

        public UserService(
            IClusterClient clusterClient,
            IEventPublisher eventPublisher,
            IJobService jobService,
            ICacheService cacheService)
        {
            _clusterClient = clusterClient;
            _eventPublisher = eventPublisher;
            _jobService = jobService;
            _cacheService = cacheService;
        }

        public async Task<Result<string>> CreateUserAsync(string email, string password, UserProfile profile)
        {
            var grain = _clusterClient.GetGrain<IUserGrain>(email);
            var result = await grain.CreateAsync(email, password, profile);

            if (result.IsSuccess)
            {
                await _eventPublisher.PublishAsync(new UserCreatedEvent
                {
                    Email = email,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName
                });
            }

            return result;
        }

        public async Task<Result<UserProfile>> GetProfileAsync(string userId)
        {
            return await _cacheService.GetOrSetAsync(
                $"user:profile:{userId}",
                async () =>
                {
                    var grain = _clusterClient.GetGrain<IUserGrain>(userId);
                    var state = await grain.GetStateAsync();
                    return Result<UserProfile>.Success(state.Profile);
                },
                TimeSpan.FromMinutes(5));
        }

        public async Task<Result<Unit>> UpdateProfileAsync(string userId, UserProfile profile)
        {
            var grain = _clusterClient.GetGrain<IUserGrain>(userId);
            var result = await grain.UpdateProfileAsync(profile);

            if (result.IsSuccess)
            {
                await _eventPublisher.PublishAsync(new UserProfileUpdatedEvent
                {
                    Email = profile.Email,
                    ChangedProperties = new Dictionary<string, string>
                    {
                        { "FirstName", profile.FirstName },
                        { "LastName", profile.LastName }
                    }
                });

                await _cacheService.RemoveAsync($"user:profile:{userId}");
            }

            return result;
        }

        public async Task<Result<string>> RequestDataExportAsync(
            string userId,
            string outputFormat,
            string notificationEmail)
        {
            var job = new UserDataExportJob
            {
                UserId = userId,
                OutputFormat = outputFormat,
                NotificationEmail = notificationEmail
            };

            var jobId = await _jobService.EnqueueAsync(job);
            return Result<string>.Success(jobId);
        }
    }
}