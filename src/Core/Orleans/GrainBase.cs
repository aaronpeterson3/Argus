using Orleans.Runtime;

namespace Argus.Core.Orleans
{
    public abstract class GrainBase : Orleans.Grain
    {
        protected readonly ILogger Logger;

        protected GrainBase(ILogger logger)
        {
            Logger = logger;
        }
    }
}