using System;
using System.Threading;
using System.Threading.Tasks;
using HcloudMetallb.DiffIP;
using HcloudMetallbController.Options;
using HcloudMetallbController.Queues;
using K8sSlimApi.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace HcloudMetallbController.Repositories;

sealed class Synchronization(IServiceProvider ServiceProvider, WorkItemQueue<ServiceAnnouncement<ServiceL2Status>> Queue, RetryQueue<ServiceAnnouncement<ServiceL2Status>> RetryQueue, IOptions<MetallbOptions> metallbOptions, ILogger<Synchronization> Logger) : BackgroundService
{
    public const int MessageMaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Yield();
        await Consumer(ct);
    }

    private async Task Consumer(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var msg = await Queue.Pop(ct);

            var service = msg.Data.ServiceStatus;
            using (LogContext.PushProperty("announcementName", service.Metadata.Name))
            using (LogContext.PushProperty("serviceNamespace", service.Status?.ServiceNamespace))
            using (LogContext.PushProperty("serviceName", service.Status?.ServiceName))
            {
                Logger.LogTrace("Announcement processing");

                if (string.IsNullOrEmpty(service.Status?.ServiceNamespace) || string.IsNullOrEmpty(service.Status?.ServiceName))
                {
                    Logger.LogWarning("No service allocated to announcement, skipping");
                    continue;
                }

                await using var scope = ServiceProvider.CreateAsyncScope();
                try
                {
                    var differ = scope.ServiceProvider.GetRequiredService<IManifestCreator>();
                    var manifestInfo = await differ.DetermineManifestAsync(service, metallbOptions.Value.Namespace, ct);
                    if (manifestInfo is null)
                    {
                        if (msg.Retries < MessageMaxRetries)
                        {
                            Logger.LogError("Empty manifest error ({retry} times)", msg.Retries);
                            await RetryQueue.Push(msg);
                        }
                        else
                        {
                            Logger.LogCritical("Empty manifest error (final; {retry} times)", msg.Retries);
                        }
                        continue;
                    }
                    var _ = await differ.ReallocateAsync(manifestInfo.Value.Manifest, manifestInfo.Value.Servers, dryRun: false, ct);
                }
                catch (Exception e)
                {
                    if (msg.Retries < MessageMaxRetries)
                    {
                        Logger.LogError("Failure ({retry} times): {error}", msg.Retries, e.Message);
                        await RetryQueue.Push(msg);
                    }
                    else
                    {
                        Logger.LogCritical("Failure (final; {retry} times): {error}", msg.Retries, e.Message);
                    }
                }
            }
        }
    }
}
