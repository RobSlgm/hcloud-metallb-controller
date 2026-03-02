using System;
using System.Collections.Generic;

namespace HcloudSlimApi.Models;

public sealed class Server
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? Created { get; set; }
    public Dictionary<string, string>? Labels { get; set; }
    public PublicNet? PublicNet { get; set; }
    public List<PrivateNet>? PrivateNet { get; set; }
}
