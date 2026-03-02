using System.Threading;
using System.Threading.Tasks;
using HcloudSlimApi;
using HcloudSlimApi.Models;

namespace HcloudMetallb.Models;

public interface IAssignIP
{
    long ServerId { get; }
    bool IsSynced { get; }
    Task<ApiResult<Action>> Assign(HcloudClient hcloud, CancellationToken ct);
}

