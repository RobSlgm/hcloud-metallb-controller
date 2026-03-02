using System;

namespace HcloudMetallbController.Queues;

public sealed class WorkItem<T> where T : notnull, new()
{
    public required T Data { get; set; }
    public required DateTime Created { get; set; }
    public int Retries { get; set; }
}
