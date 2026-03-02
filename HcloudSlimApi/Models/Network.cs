using System;
using System.Collections.Generic;

namespace HcloudSlimApi.Models;

public sealed class Network
{
    public long Id { get; set; }
    public string? Command { get; set; }
    public string? IpRange { get; set; }
    public List<long>? Servers { get; set; }
    public List<long>? LoadBalancers { get; set; }
    public Dictionary<string, string>? Labels { get; set; }
    public DateTimeOffset? Created { get; set; }
}
