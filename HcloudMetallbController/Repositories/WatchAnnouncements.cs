using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HcloudMetallbController.Options;
using HcloudMetallbController.Queues;
using k8s;
using K8sSlimApi.Coordinations;
using K8sSlimApi.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HcloudMetallbController.Repositories;

sealed class ServiceAnnouncement<T> where T : notnull
{
    public WatchEventType EventType { get; set; }
    public T ServiceStatus { get; set; } = default!;
}

sealed class WatchAnnoncements(IKubernetes k8s, WorkItemQueue<ServiceAnnouncement<ServiceL2Status>> Queue, IOptions<MetallbOptions> metallbOptions, ILogger<WatchAnnoncements> Logger) : ILeaderOperation
{
    public async Task Process(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var watcher = k8s.ServiceL2Status.WatchNamespacedAsync<ServiceL2Status>(metallbOptions.Value.Namespace, onError: (ex) => { Logger.LogCritical("Error {error}", ex.Message); }, cancel: ct);
                await foreach (var (evt, item) in watcher.WithCancellation(ct).ConfigureAwait(false))
                {
                    Logger.LogInformation("ServiceL2Status {statusName}: {evt} {serviceNamespace}/{serviceName}", item.Metadata.Name, evt, item.Status?.ServiceNamespace, item.Status?.ServiceName);
                    if (item is not null)
                    {
                        var announcement = new ServiceAnnouncement<ServiceL2Status> { EventType = evt, ServiceStatus = item, };
                        await Queue.Push(announcement);
                    }
                    else
                    {
                        Logger.LogWarning("ServiceL2Status: {evt} with no payload", evt);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Logger.LogWarning("Watch has been cancelled");
            }
            catch (HttpIOException ioe)
            {
                Logger.LogWarning("Watch IO failed: {error}", ioe.Message);
            }
            catch (Exception e)
            {
                Logger.LogError("Watch failed: {error}", e.Message);
                if (!ct.IsCancellationRequested) await Task.Delay(1000, ct); // TODO: Backoff
            }
        }
    }
}
