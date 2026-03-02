using System.Text.Json.Serialization;
using HcloudSlimApi.Utils;

namespace HcloudMetallb.Models;

[JsonConverter(typeof(JsonSnakeCaseEnumConverter<PoolType>))]
public enum PoolType
{
    FloatingIp,
    AliasIp,
}
