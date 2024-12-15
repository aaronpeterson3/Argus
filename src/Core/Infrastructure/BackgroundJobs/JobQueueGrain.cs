using Orleans;
using Orleans.Runtime;

namespace Argus.Core.Infrastructure.BackgroundJobs
{
    public interface IJobQueueGrain : IGrainWithIntegerKey
    {
        Task<string> EnqueueAsync(IJob job);
        Task<IEnumerable<IJob>> GetJobsAsync(string tenantId);
        Task ProcessNextAsync();
    }

    public class JobQueueGrain : Grain, IJobQueueGrain
    {
        private readonly IPersistentState<QueueState> _state;
        private readonly IGrainFactory _grainFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobQueueGrain> _logger;
        private IDisposable _timer;

        public JobQueueGrain(
            [PersistentState("queue")] IPersistentState<QueueState> state,
            IGrainFactory grainFactory,
            IServiceProvider serviceProvider,
            ILogger<JobQueueGrain> logger)
        {
            _state = state;
            _grainFactory = grainFactory;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _timer = RegisterTimer(
                _ => ProcessNextAsync(),
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10));

            return base.OnActivateAsync(cancellationToken);
        }

        public async Task<string> EnqueueAsync(IJob job)
        {
            var jobId = Guid.NewGuid().ToString();
            var jobState = new JobState
            {
                Id = jobId,
                Name = job.Name,
                TenantId = job.TenantId,
                Status = JobStatus.Queued,
                CreatedAt = DateTime.UtcNow
            };

            _state.State.QueuedJobs.Add(jobId);
            await _state.WriteStateAsync();

            var jobGrain = _grainFactory.GetGrain<IJobGrain>(jobId);
            await jobGrain.UpdateStatusAsync(JobStatus.Queued);

            return jobId;
        }

        public async Task<IEnumerable<IJob>> GetJobsAsync(string tenantId)
        {
            var jobs = new List<IJob>();

            foreach (var jobId in _state.State.QueuedJobs.Union(_state.State.ProcessedJobs))
            {
                var jobGrain = _grainFactory.GetGrain<IJobGrain>(jobId);
                var job = await jobGrain.GetStatusAsync();
                
                if (job.TenantId == tenantId)
                {
                    jobs.Add(job);
                }
            }

            return jobs;
        }

        public async Task ProcessNextAsync()
        {
            if (!_state.State.QueuedJobs.Any()) return;

            var jobId = _state.State.QueuedJobs.First();
            var jobGrain = _grainFactory.GetGrain<IJobGrain>(jobId);
            var job = await jobGrain.GetStatusAsync();

            try
            {
                var processorType = Type.GetType($"Argus.Features.{job.Name}Job");
                if (processorType == null)
                {
                    throw new Exception($"No processor found for job type {job.Name}");
                }

                var processor = ActivatorUtilities.CreateInstance(_serviceProvider, processorType) as IJobProcessor<IJob>;
                if (processor == null)
                {
                    throw new Exception($"Invalid processor type for {job.Name}");
                }

                await jobGrain.SetStartedAsync();
                await processor.ProcessAsync(job, CancellationToken.None);
                await jobGrain.SetCompletedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job {JobId}", jobId);
                await jobGrain.SetFailedAsync(ex.Message);
            }

            _state.State.QueuedJobs.Remove(jobId);
            _state.State.ProcessedJobs.Add(jobId);
            await _state.WriteStateAsync();
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return base.OnDeactivateAsync(reason, cancellationToken);
        }
    }

    public class QueueState
    {
        public HashSet<string> QueuedJobs { get; set; } = new();
        public HashSet<string> ProcessedJobs { get; set; } = new();
    }
}