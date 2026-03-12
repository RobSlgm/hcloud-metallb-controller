using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HcloudMetallb.Models;
using HcloudSlimApi;
using HcloudSlimApi.Models;
using k8s;
using K8sSlimApi.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RestSharp.Extensions.DependencyInjection;

namespace HcloudMetallb.DiffIP;

public interface IManifestCreator
{
    Task<(AnnounceManifest Manifest, List<Server> Servers)?> DetermineManifestAsync(ServiceL2Status announcement, string metallbNs, CancellationToken ct);
    Task<List<IAssignIP>?> ReallocateAsync(AnnounceManifest manifest, List<Server> servers, bool dryRun, CancellationToken ct);
}

public sealed class ManifestCreator(IKubernetes k8s, IRestClientFactory restClientFactory, [FromKeyedServices("Kubernetes")] ResiliencePipeline pipeline, ILogger<ManifestCreator> Logger) : IManifestCreator
{
    private HcloudClient? Hcloud;

    public async Task<(AnnounceManifest Manifest, List<Server> Servers)?> DetermineManifestAsync(ServiceL2Status announcement, string metallbNs, CancellationToken ct)
    {
        var manifest = await LoadManifest(k8s, announcement, metallbNs, ct);
        if (manifest is null) return default;

        Logger.LogTrace("Using IP out of {poolName}", manifest.Poolname);

        Hcloud ??= restClientFactory.CreateHcloudClient();
        var serverResponse = await Hcloud.Servers.List(ct);
        if (!serverResponse.IsSuccessful || serverResponse?.Data is null)
        {
            Logger.LogError("Reading server list failed: {httpStatus} {error}", serverResponse?.Status, serverResponse?.ErrorMessage ?? "");
            return default;
        }
        var servers = serverResponse.Data.Servers;
        if (servers is null)
        {
            Logger.LogError("Empty hcloud server list");
            return default;
        }
        manifest.Server = servers.FirstOrDefault(s => !string.IsNullOrEmpty(s.Name) && string.Compare(s.Name, 0, manifest.Nodename, 0, Math.Min(manifest.Nodename.Length, s.Name.Length), StringComparison.OrdinalIgnoreCase) == 0);
        if (manifest.Server is null)
        {
            Logger.LogError("Failed to map matching hcloud server");
            return default;
        }
        return (manifest, servers);
    }

    public async Task<List<IAssignIP>?> ReallocateAsync(AnnounceManifest manifest, List<Server> servers, bool dryRun, CancellationToken ct)
    {
        Hcloud ??= restClientFactory.CreateHcloudClient();
        List<IAssignIP>? actionList = manifest.PoolType switch
        {
            PoolType.AliasIp => await SyncAliasIp(Hcloud, manifest, servers, ct),
            PoolType.FloatingIp => await SyncFloatingIp(Hcloud, manifest, ct),
            _ => null,
        };
        if (actionList is null)
        {
            Logger.LogError("Failure in checking IP out of {poolName} assignment", manifest.Poolname);
            return default;
        }
        foreach (var assignIp in actionList)
        {
            if (!assignIp.IsSynced)
            {
                var assignResult = await assignIp.Assign(Hcloud, ct);
                if (!assignResult.IsSuccessful)
                {
                    Logger.LogError("Failed to synchronize IP {poolName} assignment", manifest.Poolname);
                }
                Logger.LogInformation("IP out of {poolName} moved to {nodeName}", manifest.Poolname, manifest.Nodename);
            }
            else
            {
                Logger.LogInformation("IP out of {poolName} already attached to {nodeName}", manifest.Poolname, manifest.Nodename);
            }
        }
        return actionList;
    }

    private async Task<List<IAssignIP>?> SyncFloatingIp(HcloudClient hcloud, AnnounceManifest manifest, CancellationToken ct)
    {
        var fipsResponse = await hcloud.FloatingIps.List(ct);
        if (!fipsResponse.IsSuccessful || fipsResponse?.Data is null)
        {
            Logger.LogError("Reading floating IPs failed: {httpStatus} {error}", fipsResponse?.Status, fipsResponse?.ErrorMessage ?? "");
            return default;
        }
        var fips = fipsResponse.Data.FloatingIps;
        if (fips is null || fips.Count == 0)
        {
            Logger.LogWarning("Empty floating IP list");
            return default;
        }
        var diff = new List<IAssignIP>();
        foreach (var ip in manifest.TargetIPs ?? [])
        {
            var fip = fips.FirstOrDefault(f => f.IPAddress?.Equals(ip) == true);
            if (fip is null)
            {
                Logger.LogWarning("IP {ipAddress} seems not to be a floating IP", ip.ToString());
                continue;  // this IP is not a floating IP, configuration error ?!
            }
            var sync = new AssignFloatingIP { ServerId = manifest.Server!.Id, FloatingIpId = fip.Id, IPAddress = ip, IsSynced = manifest.Server?.Id == fip.Server, };
            diff.Add(sync);
        }
        return diff;
    }

    private async Task<List<IAssignIP>?> SyncAliasIp(HcloudClient hcloud, AnnounceManifest manifest, List<Server> servers, CancellationToken ct)
    {
        var networkResponse = await hcloud.Networks.List(ct);
        if (!networkResponse.IsSuccessful || networkResponse?.Data is null)
        {
            Logger.LogError("Reading internal networks failed: {httpStatus} {error}", networkResponse?.Status, networkResponse?.ErrorMessage ?? "");
            return default;
        }
        var networks = networkResponse.Data.Networks;
        if (networks is null || networks.Count == 0)
        {
            Logger.LogWarning("Empty private networks list");
            return default;
        }
        var aliasIps = new List<AliasIP>();
        foreach (var tip in manifest.TargetIPs ?? [])
        {
            AliasIP? aliasIp = null;
            foreach (var network in networks)
            {
                if(network.IpRange is null) continue;
                if (!IPNetwork2.TryParse(network.IpRange, out var range))
                {
                    Logger.LogWarning("Failed to parse IP range {ipRange} of network #{networkId}", network.IpRange, network.Id);
                    continue;
                }
                if (range.Contains(tip))
                {
                    aliasIp = new AliasIP { IPAddress = tip, Network = network.Id, };
                    break;
                }
            }
            if (aliasIp is null)
            {
                Logger.LogWarning("No network found for IP {ip}", tip.ToString());
                return default;
            }
            aliasIps.Add(aliasIp);
        }
        var diff = new List<IAssignIP>();
        foreach (var aip in aliasIps)
        {
            var syncTarget = new AssignAliasIP
            {
                ServerId = manifest.Server!.Id,
                NetworkId = aip.Network,
                IPAddresses = GetAliasIPs(manifest.Server!, aip.Network),
            };
            syncTarget.IsSynced = !syncTarget.IPAddresses.Add(aip.IPAddress);

            // check if alias IP is used by another server and remove it there
            foreach (var server in servers.Where(s => s.Id != manifest.Server.Id))
            {
                var syncRemoval = new AssignAliasIP
                {
                    ServerId = server.Id,
                    NetworkId = aip.Network,
                    IPAddresses = GetAliasIPs(server, aip.Network),
                };
                syncRemoval.IsSynced = !syncRemoval.IPAddresses.Remove(aip.IPAddress);
                if (syncRemoval.IsSynced == false)
                {
                    diff.Add(syncRemoval);
                }
            }

            diff.Add(syncTarget);
        }
        return diff;
    }

    private static HashSet<IPAddress> GetAliasIPs(Server server, long networkId)
    {
        var addresses = new HashSet<IPAddress>();
        if (server.PrivateNet is null) return addresses;
        var network = server.PrivateNet.FirstOrDefault(pn => pn.Network == networkId);
        if (network is null) return addresses;
        foreach (var aip in network.AliasIPs ?? [])
        {
            if (IPAddress.TryParse(aip, out var address))
            {
                addresses.Add(address);
            }
        }
        return addresses;
    }

    private async Task<AnnounceManifest?> LoadManifest(IKubernetes k8s, ServiceL2Status announce, string metallbNs, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(announce.Status?.ServiceNamespace) || string.IsNullOrEmpty(announce.Status?.ServiceName))
        {
            Logger.LogError("Invalid announcement");
            return null;
        }

        var matchingService = await pipeline.ExecuteAsync(async (stoppingToken) => await k8s.CoreV1.ReadNamespacedServiceAsync(announce.Status.ServiceName, announce.Status.ServiceNamespace, cancellationToken: stoppingToken), ct);
        if (matchingService is null)
        {
            Logger.LogError("Failed to read service");
            return null;
        }

        var ipPool = matchingService.Metadata.Annotations.FirstOrDefault(a => a.Key.Equals("metallb.io/ip-allocated-from-pool", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(ipPool.Key))
        {
            Logger.LogWarning("Service has no addresspool, skipping");
            return null;
        }

        var pool = await pipeline.ExecuteAsync(async (stoppingToken) => await k8s.IPAddressPool.ReadNamespacedAsync<IPAddressPool>(metallbNs, ipPool.Value, stoppingToken), ct);
        if (pool is null)
        {
            Logger.LogWarning("Failed to read addresspool, skipping");
            return null;
        }

        var ipPoolType = pool.Metadata.Annotations.FirstOrDefault(a => a.Key.Equals("closure.ch/pool-type", StringComparison.OrdinalIgnoreCase) == true);
        if (string.IsNullOrEmpty(ipPoolType.Key))
        {
            Logger.LogWarning("Addresspool has no closure.ch/pool-type annotation, skipping");
            return null;
        }
        if (!Enum.TryParse<PoolType>(ipPoolType.Value, out var poolType))
        {
            Logger.LogWarning("closure.ch/pool-type {poolType} invalid, skipping", ipPoolType.Value);
            return null;
        }
        var targetIPs = matchingService.Status?.LoadBalancer?.Ingress?.Select(ig => IPAddress.Parse(ig.Ip)) ?? [];
        if (targetIPs.Any() == false)
        {
            Logger.LogWarning("No loadbalancer IP found in service, skipping");
            return null;
        }

        return new AnnounceManifest
        {
            Announcement = announce,
            Service = matchingService,
            Poolname = ipPool.Value,
            Nodename = announce.Status.Node,
            PoolType = poolType,
            TargetIPs = [.. targetIPs],
        };
    }

}
