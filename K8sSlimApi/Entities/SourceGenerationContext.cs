using System.Text.Json.Serialization;
using k8s.Models;
using static k8s.KubernetesJson;

namespace K8sSlimApi.Entities;

[JsonSerializable(typeof(ServiceBGPStatus))]
[JsonSerializable(typeof(ServiceL2Status))]
[JsonSerializable(typeof(IPAddressPool))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    Converters = new[] { typeof(Iso8601TimeSpanConverter), typeof(KubernetesDateTimeConverter), typeof(KubernetesDateTimeOffsetConverter), typeof(V1Status.V1StatusObjectViewConverter) })
]
public partial class SourceGenerationContext : JsonSerializerContext
{
}
