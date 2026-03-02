using System;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.LeaderElection;
using k8s.LeaderElection.ResourceLock;
using K8sSlimApi.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace K8sSlimApi.Coordinations;

public sealed class LeadCoordinator<TOperator>(IServiceProvider Services, IKubernetes k8s, IOptions<LeaderElectionOption> leaderOptions, IOptions<RuntimeOptions> runtimeOptions, ILogger<LeadCoordinator<TOperator>> Logger) : BackgroundService where TOperator : ILeaderOperation
{
    readonly CancellationTokenSource leaderCts = new();
    CancellationToken? ctService;
    LeaderElector? Elector;
    string Identity = Environment.MachineName;

    public async Task ExecuteNowAsync(CancellationToken ct) => await ExecuteAsync(ct);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (Environment.ProcessId > 1)
        {
            Identity = $"{Environment.MachineName}-{Environment.ProcessId}";
        }
        var leaderLock = new LeaseLock(k8s, runtimeOptions.Value.PodNamespace, "hcloud-metallb-controller", Identity);

        var leaderElectionConfig = new LeaderElectionConfig(leaderLock)
        {
            LeaseDuration = TimeSpan.FromSeconds(leaderOptions.Value.LeaseDurationSeconds),
            RenewDeadline = TimeSpan.FromSeconds(leaderOptions.Value.RenewDeadlineSeconds),
            RetryPeriod = TimeSpan.FromSeconds(leaderOptions.Value.RetryPeriodSeconds),
        };

        Elector = new LeaderElector(leaderElectionConfig);
        Elector.OnError += OnError;
        Elector.OnNewLeader += OnNewLeader;
        Elector.OnStartedLeading += OnLeading;
        Elector.OnStoppedLeading += OnStoppedLeading;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                ctService = ct;
                await Elector.RunUntilLeadershipLostAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                Logger.LogInformation("Leading has been cancelled");
            }
            catch (TaskCanceledException)
            {
                Logger.LogInformation("Task for leading has been cancelled");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Leader failure {error}", ex.Message);
                if (!ct.IsCancellationRequested) await Task.Delay(leaderElectionConfig.RetryPeriod, ct); // TODO: Backoff exponential
            }
        }
    }

    private async Task Process(CancellationToken ct)
    {
        int count = 0;
        await using var scope = Services.CreateAsyncScope();
        var ops = scope.ServiceProvider.GetRequiredService<TOperator>();
        while (!ct.IsCancellationRequested)
        {
            await ops.Process(ct);
            Logger.LogDebug("{leaderName}/{count}: Working {stamp:g}", Identity, count, DateTime.Now);
            ++count;
        }
    }

    private void OnLeading()
    {
        Logger.LogInformation("Leading started at {leaderName}", Identity);
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(leaderCts.Token, ctService ?? CancellationToken.None);
        _ = Process(linkedCts.Token);
    }

    private void OnStoppedLeading()
    {
        Logger.LogInformation("Leading stopped at {leaderName}", Identity);
    }

    private void OnNewLeader(string leaderName)
    {
        Logger.LogInformation("Leader is {leaderName}, I am {currentName}", leaderName, Identity);
    }

    private void OnError(Exception e)
    {
        Logger.LogError("Leader failure {error}", e.Message);
    }
}
