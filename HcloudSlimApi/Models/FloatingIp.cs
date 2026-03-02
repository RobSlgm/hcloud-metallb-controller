using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace HcloudSlimApi.Models;


public sealed class FloatingIp
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Ip { get; set; }
    public FloatingIPType Type { get; set; }
    public long Server { get; set; }
    public Dictionary<string, string>? Labels { get; set; }

    [JsonIgnore]
    public IPAddress? IPAddress
    {
        get
        {
            if (IPAddress.TryParse(Ip, out var address))
            {
                return address;
            }
            return null;
        }
    }
}
