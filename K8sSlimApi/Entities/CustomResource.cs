using System.Collections.Generic;
using System.Text.Json.Serialization;
using k8s;
using k8s.Models;


namespace K8sSlimApi.Entities;

public sealed class CustomResourceDefinition
{
    public required string Version { get; set; }

    public required string Group { get; set; }

    public required string PluralName { get; set; }

    public required string Kind { get; set; }

    public string? Namespace { get; set; }
}

public abstract class CustomResource : IKubernetesObject<V1ObjectMeta>
{
    [JsonPropertyName("apiVersion")]
    public virtual string ApiVersion { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public virtual string Kind { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = default!;
}

public abstract class CustomResource<TSpec, TStatus> : CustomResource
{
    [JsonPropertyName("spec")]
    public TSpec? Spec { get; set; }

    [JsonPropertyName("status")]
    public TStatus? Status { get; set; }
}

public class CustomResourceList<T> : IKubernetesObject<V1ListMeta>
where T : CustomResource
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "";

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("metadata")]
    public required V1ListMeta Metadata { get; set; }

    [JsonPropertyName("items")]
    public List<T>? Items { get; set; }
}
