using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi.Models;
using RestSharp;

namespace HcloudSlimApi.Apis;


public sealed class ServersApi(HcloudClient hcloud)
{
    public async Task<ApiResult<ServerList>> List(CancellationToken ct)
    {
        var request = new RestRequest("/v1/servers").AddQueryParameter("sort", "name:asc");
        var response = await hcloud.Client.ExecuteGetAsync<ServerList>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ServerList>();
    }

    public async Task<ApiResult<ServerList>> Lookup(string name, CancellationToken ct)
    {
        var request = new RestRequest("/v1/servers")
            .AddQueryParameter("name", name)
            .AddQueryParameter("sort", "name:asc");
        var response = await hcloud.Client.ExecuteGetAsync<ServerList>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ServerList>();
    }

    public async Task<ApiResult<ServerRecord>> Get(long id, CancellationToken ct)
    {
        var request = new RestRequest("/v1/servers/{id}").AddUrlSegment("id", id);
        var response = await hcloud.Client.ExecuteGetAsync<ServerRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ServerRecord>();
    }

    public async Task<ApiResult<ActionRecord>> GetAction(long actionId, CancellationToken ct)
    {
        var request = new RestRequest("/v1/servers/actions/{actionId}")
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
