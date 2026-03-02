using System.Text.Json.Serialization;
using k8s;
using k8s.Models;


namespace K8sSlimApi.Entities;

public record IPAddressPoolSpec
{
    public string[] Addresses { get; set; } = [];

    public bool AutoAssign { get; set; }

    [JsonPropertyName("avoidBuggyIPs")]
    public bool AvoidBuggyIPs { get; set; }

    // ServiceAllocation
}

public record IPAddressPoolStatus : V1Status
{
    [JsonPropertyName("assignedIPv4")]
    public int AssignedIPv4 { get; set; }

    [JsonPropertyName("assignedIPv6")]
    public int AssignedIPv6 { get; set; }

    [JsonPropertyName("availableIPv4")]
    public int AvailableIPv4 { get; set; }

    [JsonPropertyName("availableIPv6")]
    public int AvailableIPv6 { get; set; }
}

public sealed class IPAddressPool : CustomResource<IPAddressPoolSpec, IPAddressPoolStatus>
{
    public override string ApiVersion { get; set; } = "v1beta1";

    public override string Kind { get; set; } = "IPAddressPool";

    public static CustomResourceDefinition Crd
    {
        get
        {
            return new CustomResourceDefinition
            {
                Group = "metallb.io",
                Version = "v1beta1",
                Kind = "IPAddressPool",
                PluralName = "IPAddressPools",
            };
        }
    }
}

public static class IPAddressPoolExtension
{
    extension(IKubernetes k8s)
    {
        public GenericClient IPAddressPool
        {
            get
            {
                var crd = IPAddressPool.Crd;
                var generic = new GenericClient(k8s, crd.Group.ToLowerInvariant(), crd.Version, crd.PluralName.ToLowerInvariant());
                return generic;
            }
        }
    }
}
