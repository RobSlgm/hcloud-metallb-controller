namespace K8sSlimApi.Options;

public sealed class RuntimeOptions
{
    public string PodNamespace { get; set; } = "metallb-system";
    public string? PodName { get; set; }
    public string? PodServiceAccountName { get; set; }
}
