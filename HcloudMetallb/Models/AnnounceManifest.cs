using System.Collections.Generic;
using System.Net;
using HcloudSlimApi.Models;
using K8sSlimApi.Entities;

namespace HcloudMetallb.Models;

public sealed class AnnounceManifest
{
    public required ServiceL2Status Announcement { get; init; }
    public required k8s.Models.V1Service Service { get; init; }
    public required string Poolname { get; init; }
    public PoolType PoolType { get; set; } = PoolType.FloatingIp;
    public required string Nodename { get; init; }
    public Server? Server { get; set; }
    public List<IPAddress>? TargetIPs { get; set; }
}

