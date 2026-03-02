using System.Threading;
using System.Threading.Tasks;

namespace K8sSlimApi.Coordinations;

public interface ILeaderOperation
{
    Task Process(CancellationToken ct);
}
