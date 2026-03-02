using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HcloudMetallbController.Queues;

public class WorkItemQueue<T> where T : notnull, new()
{
    private readonly Channel<WorkItem<T>> Queue;

    public WorkItemQueue()
    {
        var options = new UnboundedChannelOptions { SingleReader = true, SingleWriter = false, };
        Queue = Channel.CreateUnbounded<WorkItem<T>>(options);
    }

    public async Task Push(T msg)
    {
        await Queue.Writer.WriteAsync(new() { Data = msg, Created = DateTime.UtcNow, Retries = 0, });
    }

    public async Task Retry(WorkItem<T> msg)
    {
        msg.Retries++;
        await Queue.Writer.WriteAsync(msg);
    }


    public async Task<WorkItem<T>> Pop(CancellationToken ct)
    {
        return await Queue.Reader.ReadAsync(ct);
    }
}
