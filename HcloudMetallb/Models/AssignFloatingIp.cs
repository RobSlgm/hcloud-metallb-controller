using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi;
using HcloudSlimApi.Models;

namespace HcloudMetallb.Models;

public sealed class AssignFloatingIP : IAssignIP
{
    public long ServerId { get; set; }
    public long FloatingIpId { get; set; }
    public required IPAddress IPAddress { get; set; }
    public bool IsSynced { get; set; }

    public async Task<ApiResult<Action>> Assign(HcloudClient hcloud, CancellationToken ct)
    {
        var result = await hcloud.FloatingIps.Assign(FloatingIpId, ServerId, ct);
        if (result is null || result.Data is null || !result.IsSuccessful)
        {
            return new ApiResult<Action>(IsSuccessful: false, Status: result?.Status ?? HttpStatusCode.InternalServerError, Uri: result?.Uri, ErrorMessage: result?.ErrorMessage);
        }
        return await hcloud.Actions.WaitFor(result.Data, ct);
    }
}
