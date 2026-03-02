using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HcloudMetallbController.Queues;

public class RetryQueue<T> where T : notnull, new()
{
    private readonly PriorityQueue<WorkItem<T>, DateTime> Queue = new();
    public const int MsgDelayInSeconds = 8;

    public async Task Push(WorkItem<T> msg)
    {
        Queue.Enqueue(msg, DateTime.UtcNow.AddSeconds(MsgDelayInSeconds * (1 + msg.Retries)));
    }

    public async Task<(WorkItem<T>? Message, TimeSpan Delay)> TryDequeue(CancellationToken ct)
    {
        if (Queue.TryPeek(out _, out var readyTime))
        {
            var delay = readyTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return (default, delay);
            }
            var msg = Queue.Dequeue();
            return (msg, TimeSpan.Zero);
        }
        return (default, TimeSpan.FromSeconds(MsgDelayInSeconds));
    }
}
