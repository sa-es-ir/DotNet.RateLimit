using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.RateLimit
{
    public interface IRateLimitBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken);
    }
}