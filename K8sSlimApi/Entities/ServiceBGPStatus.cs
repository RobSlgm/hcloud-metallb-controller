using k8s;
using k8s.Models;


namespace K8sSlimApi.Entities;

public record ServiceBGPStatusSpec
{
}

public record MetalLBServiceBGPStatusStatus : V1Status
{
    public string Node { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceNamespace { get; set; } = string.Empty;
    public string[] Peers { get; set; } = [];
}

// [KubernetesEntity(Group = "metallb.io", ApiVersion = "v1beta1", Kind = "ServiceBGPStatus", PluralName = "ServiceBGPStatuses")]
public sealed class ServiceBGPStatus : CustomResource<ServiceBGPStatusSpec, MetalLBServiceBGPStatusStatus>
{
    public override string ApiVersion { get; set; } = "v1beta1";

    public override string Kind { get; set; } = "ServiceBGPStatus";

    public static CustomResourceDefinition Crd
    {
        get
        {
            return new CustomResourceDefinition
            {
                Group = "metallb.io",
                Version = "v1beta1",
                Kind = "ServiceBGPStatus",
                PluralName = "ServiceBGPStatuses",
            };
        }
    }
}

public static class ServiceBGPStatusEntityExtension
{
    extension(IKubernetes k8s)
    {
        public GenericClient ServiceBGPStatus
        {
            get
            {
                var crd = ServiceBGPStatus.Crd;
                var generic = new GenericClient(k8s, crd.Group.ToLowerInvariant(), crd.Version, crd.PluralName.ToLowerInvariant());
                return generic;
            }
        }
    }
}
