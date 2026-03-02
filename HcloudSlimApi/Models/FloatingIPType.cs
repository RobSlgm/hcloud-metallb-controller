using System.Text.Json.Serialization;
using HcloudSlimApi.Utils;

namespace HcloudSlimApi.Models;

[JsonConverter(typeof(JsonSnakeCaseEnumConverter<FloatingIPType>))]
public enum FloatingIPType
{
    Undefined,
    Ipv4,
    Ipv6,
}
