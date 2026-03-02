using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi;
using HcloudSlimApi.Models;

namespace HcloudMetallb.Models;

public sealed class AssignAliasIP : IAssignIP
{
    public long ServerId { get; set; }
    public HashSet<IPAddress> IPAddresses { get; set; } = [];
    public long NetworkId { get; set; }
    public bool IsSynced { get; set; }

    public async Task<ApiResult<Action>> Assign(HcloudClient hcloud, CancellationToken ct)
    {
        var result = await hcloud.AliasIps.Assign(ServerId, [.. IPAddresses], NetworkId, ct);
        if (result is null || result.Data is null || !result.IsSuccessful)
        {
            return new ApiResult<Action>(IsSuccessful: false, Status: result?.Status ?? HttpStatusCode.InternalServerError, Uri: result?.Uri, ErrorMessage: result?.ErrorMessage);
        }
        return await hcloud.Actions.WaitFor(result.Data, ct);
    }
}
