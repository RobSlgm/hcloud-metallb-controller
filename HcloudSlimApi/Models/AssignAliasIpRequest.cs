using System.Collections.Generic;

namespace HcloudSlimApi.Models;

public sealed class AssignAliasIpRequest
{
    public required long Network { get; set; }
    public required List<string> AliasIps { get; set; }
}
