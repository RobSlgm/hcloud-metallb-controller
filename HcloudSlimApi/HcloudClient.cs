using System;
using System.Text.Json;
using HcloudSlimApi.Options;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Extensions.DependencyInjection;
using RestSharp.Serializers.Json;

namespace HcloudSlimApi;

public sealed class HcloudClient(IRestClient client)
{
    internal IRestClient Client { get { return client; } }
}


public static class HcloudClientExtensions
{
    extension(IRestClientFactory factory)
    {
        public HcloudClient CreateHcloudClient()
        {
            var client = factory.CreateClient(nameof(HcloudClient));
            return new HcloudClient(client);
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddHcloudApiClient(HcloudOptions? hcloudOptions)
        {

            services.AddHttpClient(
                nameof(HcloudClient)).AddStandardResilienceHandler(options =>
                {
                    options.Retry.Delay = TimeSpan.FromSeconds(2);
                    options.Retry.UseJitter = true;
                    options.Retry.BackoffType = DelayBackoffType.Exponential;
                }
            );
            services.AddRestClient(
                nameof(HcloudClient),
                options =>
                {
                    if (hcloudOptions is null)
                    {
                        throw new InvalidOperationException("Hcloud configuration is required");
                    }
                    if (string.IsNullOrEmpty(hcloudOptions.ApiKey))
                    {
                        throw new InvalidOperationException("Hcloud:ApiKey is required");
                    }
                    options.BaseUrl = new Uri(hcloudOptions.BaseUrl);
                    options.Timeout = TimeSpan.FromSeconds(hcloudOptions.TimeoutSeconds);
                    options.FailOnDeserializationError = true;
                    options.Authenticator = new JwtAuthenticator(hcloudOptions.ApiKey);
                },
                s => s.UseSystemTextJson(new JsonSerializerOptions
                {
                    TypeInfoResolver = Models.SourceGenerationContext.Default,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                })
            );

            return services;
        }
    }
}
