using System;
using System.Text.Json.Serialization.Metadata;
using HcloudMetallb.DiffIP;
using k8s;
using K8sSlimApi.Options;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace HcloudMetallb;

public static class StartupExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKubernetesApiClient()
        {
            services.AddResiliencePipeline(nameof(Kubernetes), builder =>
            {
                builder.AddRetry(new()
                {
                    ShouldHandle = new PredicateBuilder()
                        // .Handle<HttpRequestException>()
                        // .Handle<HttpIOException>()
                        .Handle<Exception>(),
                        // .HandleResult(response => response.StatusCode == HttpStatusCode.TooManyRequests),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                })
                .AddTimeout(TimeSpan.FromSeconds(1.5));
            });
            services.Configure<RuntimeOptions>(o =>
            {
                o.PodNamespace = Environment.GetEnvironmentVariable("POD_NAMESPACE") ?? "default";
                o.PodName = Environment.GetEnvironmentVariable("POD_NAME");
                o.PodServiceAccountName = Environment.GetEnvironmentVariable("POD_SERVICE_ACCOUNT_NAME");
            });
            KubernetesJson.AddJsonOptions(options =>
            {
                options.TypeInfoResolver = JsonTypeInfoResolver.Combine(k8s.SourceGenerationContext.Default, K8sSlimApi.Entities.SourceGenerationContext.Default, options.TypeInfoResolver);
            });
            var k8sConfig = KubernetesClientConfiguration.BuildDefaultConfig();
            services.AddSingleton<IKubernetes>(_ => new Kubernetes(k8sConfig));
            return services;
        }

        public IServiceCollection AddHcloudMetallbController()
        {
            services.AddScoped<IManifestCreator, ManifestCreator>();
            return services;
        }
    }
}
