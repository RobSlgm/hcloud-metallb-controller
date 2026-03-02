using System.Net;

namespace HcloudMetallb.Models;

public sealed class AliasIP
{
    public long Network { get; set; }
    public required IPAddress IPAddress { get; set; }
}

