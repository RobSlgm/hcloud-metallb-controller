using k8s;
using k8s.Models;


namespace K8sSlimApi.Entities;

public record ServiceL2StatusSpec
{
}

public record MetalLBServiceL2StatusStatus : V1Status
{
    public string Node { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceNamespace { get; set; } = string.Empty;
}


// [KubernetesEntity(Group = "metallb.io", ApiVersion = "v1beta1", Kind = "ServiceL2Status", PluralName = "ServiceL2Statuses")]
public sealed class ServiceL2Status : CustomResource<ServiceBGPStatusSpec, MetalLBServiceBGPStatusStatus>
{
    public override string ApiVersion { get; set; } = "v1beta1";

    public override string Kind { get; set; } = "ServiceL2Status";

    public static CustomResourceDefinition Crd
    {
        get
        {
            return new CustomResourceDefinition
            {
                Group = "metallb.io",
                Version = "v1beta1",
                Kind = "ServiceL2Status",
                PluralName = "ServiceL2Statuses",
            };
        }
    }
}


public static class ServiceL2StatusEntityExtension
{
    extension(IKubernetes k8s)
    {
        public GenericClient ServiceL2Status
        {
            get
            {
                var crd = ServiceL2Status.Crd;
                var generic = new GenericClient(k8s, crd.Group.ToLowerInvariant(), crd.Version, crd.PluralName.ToLowerInvariant());
                return generic;
            }
        }
    }
}
