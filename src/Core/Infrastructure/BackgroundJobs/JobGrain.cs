using Orleans;

namespace Argus.Core.Infrastructure.BackgroundJobs
{
    public interface IJobGrain : IGrainWithStringKey
    {
        Task<IJob> GetStatusAsync();
        Task UpdateStatusAsync(JobStatus status, string error = null);
        Task SetStartedAsync();
        Task SetCompletedAsync();
        Task SetFailedAsync(string error);
    }

    public class JobGrain : Grain, IJobGrain
    {
        private readonly IPersistentState<JobState> _state;

        public JobGrain([PersistentState("job")] IPersistentState<JobState> state)
        {
            _state = state;
        }

        public Task<IJob> GetStatusAsync()
        {
            return Task.FromResult<IJob>(_state.State);
        }

        public async Task UpdateStatusAsync(JobStatus status, string error = null)
        {
            _state.State.Status = status;
            _state.State.Error = error;

            if (status == JobStatus.Running)
            {
                _state.State.StartedAt = DateTime.UtcNow;
            }
            else if (status == JobStatus.Completed || status == JobStatus.Failed)
            {
                _state.State.CompletedAt = DateTime.UtcNow;
            }

            await _state.WriteStateAsync();
        }

        public Task SetStartedAsync()
        {
            return UpdateStatusAsync(JobStatus.Running);
        }

        public Task SetCompletedAsync()
        {
            return UpdateStatusAsync(JobStatus.Completed);
        }

        public Task SetFailedAsync(string error)
        {
            return UpdateStatusAsync(JobStatus.Failed, error);
        }
    }

    public class JobState : IJob
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TenantId { get; set; }
        public JobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Error { get; set; }
    }
}