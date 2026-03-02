using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi.Models;
using RestSharp;

namespace HcloudSlimApi.Apis;


public sealed class FloatingIpsApi(HcloudClient hcloud)
{
    public async Task<ApiResult<FloatingIpList>> List(CancellationToken ct)
    {
        var request = new RestRequest("/v1/floating_ips").AddQueryParameter("sort", "id:asc");
        var response = await hcloud.Client.ExecuteGetAsync<FloatingIpList>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<FloatingIpList>();
    }

    public async Task<ApiResult<FloatingIpRecord>> Get(long id, CancellationToken ct)
    {
        var request = new RestRequest("/v1/floating_ips/{id}").AddUrlSegment("id", id);
        var response = await hcloud.Client.ExecuteGetAsync<FloatingIpRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<FloatingIpRecord>();
    }

    public async Task<ApiResult<Action>> Assign(long id, long serverId, CancellationToken ct)
    {
        var request = new RestRequest("/v1/floating_ips/{id}/actions/assign")
            .AddUrlSegment("id", id)
            .AddBody(new AssignFloatingIpRequest
            {
                Server = serverId
            });
        var response = await hcloud.Client.ExecutePostAsync<ActionRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Action);
        }
        return response.Nok<Action>();
    }

    public async Task<ApiResult<ActionList>> ListActions(long id, CancellationToken ct)
    {
        var request = new RestRequest("/v1/floating_ips/{id}/actions").AddUrlSegment("id", id);
        var response = await hcloud.Client.ExecuteGetAsync<ActionList>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ActionList>();
    }

    public async Task<ApiResult<Action>> GetAction(long id, long actionId, CancellationToken ct)
    {
        var request = new RestRequest("/v1/floating_ips/{id}/actions/{actionId}")
            .AddUrlSegment("id", id)
            .AddUrlSegment("actionId", actionId)
            ;
        var response = await hcloud.Client.ExecuteGetAsync<ActionRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Action);
        }
        return response.Nok<Action>();
    }
}
