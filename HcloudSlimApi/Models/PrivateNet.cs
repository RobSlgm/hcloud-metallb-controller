using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HcloudSlimApi.Models;

public sealed class PrivateNet
{
    public long Network { get; set; }
    public string? Ip { get; set; }
    public string? MacAddress { get; set; }
    [JsonPropertyName("alias_ips")]
    public List<string>? AliasIPs { get; set; }
}
