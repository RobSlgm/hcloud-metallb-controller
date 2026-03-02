using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi.Models;
using RestSharp;

namespace HcloudSlimApi.Apis;


public sealed class AliasIpApi(HcloudClient hcloud)
{
    public async Task<ApiResult<Action>> Assign(long serverId, List<IPAddress> addresses, long networkId, CancellationToken ct)
    {
        var request = new RestRequest("/v1/servers/{id}/actions/change_alias_ips")
            .AddUrlSegment("id", serverId)
            .AddBody(new AssignAliasIpRequest
            {
                AliasIps = [.. addresses.Select(a => a.ToString())],
                Network = networkId,
            })
            ;
        var response = await hcloud.Client.ExecutePostAsync<ActionRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Action);
        }
        return response.Nok<Action>();
    }
}
