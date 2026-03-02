using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HcloudMetallbController.Queues;

public class RetryQueueService<TMsg>(WorkItemQueue<TMsg> Queue, RetryQueue<TMsg> RetryQueue) : BackgroundService where TMsg : notnull, new()
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var (Message, Delay) = await RetryQueue.TryDequeue(ct);
            if (Message is not null)
            {
                await Queue.Retry(Message);
                continue;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(Math.Min(Delay.TotalMilliseconds, 250), 1000)), ct);
        }
    }
}
