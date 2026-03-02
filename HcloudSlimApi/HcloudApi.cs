using HcloudSlimApi.Apis;

namespace HcloudSlimApi;


public static class HcloudApi
{
    extension(HcloudClient hcloud)
    {
        public FloatingIpsApi FloatingIps { get { return new FloatingIpsApi(hcloud); } }
        public AliasIpApi AliasIps { get { return new AliasIpApi(hcloud); } }
        public NetworksApi Networks { get { return new NetworksApi(hcloud); } }
        public ServersApi Servers { get { return new ServersApi(hcloud); } }
        public ActionsApi Actions { get { return new ActionsApi(hcloud); } }
    }
}
