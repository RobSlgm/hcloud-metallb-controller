using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi.Models;
using RestSharp;

namespace HcloudSlimApi.Apis;


public sealed class NetworksApi(HcloudClient hcloud)
{
    public async Task<ApiResult<NetworkList>> List(CancellationToken ct)
    {
        var request = new RestRequest("/v1/networks").AddQueryParameter("sort", "name:asc");
        var response = await hcloud.Client.ExecuteGetAsync<NetworkList>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<NetworkList>();
    }

    public async Task<ApiResult<NetworkList>> Lookup(string name, CancellationToken ct)
    {
        var request = new RestRequest("/v1/servers")
            .AddQueryParameter("name", name)
            .AddQueryParameter("sort", "name:asc");
        var response = await hcloud.Client.ExecuteGetAsync<NetworkList>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<NetworkList>();
    }

    public async Task<ApiResult<NetworkRecord>> Get(long id, CancellationToken ct)
    {
        var request = new RestRequest("/v1/networks/{id}").AddUrlSegment("id", id);
        var response = await hcloud.Client.ExecuteGetAsync<NetworkRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<NetworkRecord>();
    }

    public async Task<ApiResult<ActionRecord>> GetAction(long actionId, CancellationToken ct)
    {
        var request = new RestRequest("/v1/networks/actions/{actionId}")
            .AddUrlSegment("actionId", actionId)
            ;
        var response = await hcloud.Client.ExecuteGetAsync<ActionRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ActionRecord>();
    }
}
