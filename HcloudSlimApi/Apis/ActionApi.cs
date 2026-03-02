using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi.Models;
using RestSharp;

namespace HcloudSlimApi.Apis;


public sealed class ActionsApi(HcloudClient hcloud)
{
    public async Task<ApiResult<Models.Action>> Get(long id, CancellationToken ct)
    {
        var request = new RestRequest("/v1/actions/{id}").AddUrlSegment("id", id);
        var response = await hcloud.Client.ExecuteGetAsync<ActionRecord>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Action);
        }
        return response.Nok<Models.Action>();
    }

    public async Task<ApiResult<Models.Action>> WaitFor(Models.Action action, CancellationToken ct)
    {
        int maxRetryAttempts = 10;
        int retryCount = 0;
        var backoff = new ExponentialBackoff(1, 60, 2, true);
        ApiResult<Action>? lastResult;
        do
        {
            if (action.IsFinal)
            {
                return new ApiResult<Action>(true, Data: action);
            }
            await Task.Delay(backoff.NextDelay(++retryCount), ct);
            lastResult = await Get(action.Id, ct);
            if (lastResult.IsSuccessful)
            {
                if (lastResult.Data is not null) action = lastResult.Data;
            }
        }
        while (retryCount < maxRetryAttempts && !ct.IsCancellationRequested);
        return new ApiResult<Action>(false, lastResult.Status ?? System.Net.HttpStatusCode.InternalServerError, Data: action, ErrorMessage: lastResult.ErrorMessage);
    }
}
