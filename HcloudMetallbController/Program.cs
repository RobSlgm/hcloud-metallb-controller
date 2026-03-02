using System;
using HcloudMetallb;
using HcloudMetallbController.Options;
using HcloudMetallbController.Queues;
using HcloudMetallbController.Repositories;
using HcloudSlimApi;
using HcloudSlimApi.Options;
using k8s.Exceptions;
using K8sSlimApi.Coordinations;
using K8sSlimApi.Entities;
using K8sSlimApi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Hcloud Metallb Controller {version}", ThisAssembly.AssemblyInformationalVersion.Split('+')[0]);

    var builder = WebApplication.CreateSlimBuilder(args);

    var isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
    builder.Services.AddSerilog((services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services);
        if (isRunningInContainer)
        {
            loggerConfiguration
                .Enrich.WithK8sPodNamespace(DownwardApiMethod.EnvironmentVariable)
                .Enrich.WithK8sPodName(DownwardApiMethod.EnvironmentVariable)
                .Enrich.WithK8sPodServiceAccountName(DownwardApiMethod.EnvironmentVariable);
        }
        loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.Console(new RenderedCompactJsonFormatter());
    });

    builder.Services.Configure<MetallbOptions>(builder.Configuration.GetSection("Metallb"));
    builder.Services.Configure<LeaderElectionOption>(builder.Configuration.GetSection("LeaderElection"));

    builder.Services.AddHcloudApiClient(builder.Configuration.GetSection("Hcloud").Get<HcloudOptions>());

    builder.Services.AddKubernetesApiClient();

    builder.Services.AddSingleton<WorkItemQueue<ServiceAnnouncement<ServiceL2Status>>>();
    builder.Services.AddSingleton<RetryQueue<ServiceAnnouncement<ServiceL2Status>>>();
    builder.Services.AddScoped<WatchAnnoncements>();
    builder.Services.AddHcloudMetallbController();

    builder.Services.AddHostedService<RetryQueueService<ServiceAnnouncement<ServiceL2Status>>>();
    builder.Services.AddHostedService<LeadCoordinator<WatchAnnoncements>>();
    builder.Services.AddHostedService<Synchronization>();

    var app = builder.Build();

    // var healthApi = app.MapGroup("/health");
    // healthApi.MapGet("/", Results<Ok, BadRequest> (IApplicationOperationState operationState) =>
    // {
    //     if (operationState.IsHealty(5))
    //     {
    //         return TypedResults.Ok();
    //     }
    //     return TypedResults.BadRequest();
    // }).WithName("GetHealth");

    await app.RunAsync();
}
catch (KubeConfigException)
{
    Log.Fatal("Failed to connect to kubernetes (config invalid)");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
