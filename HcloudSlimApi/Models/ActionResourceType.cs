using System.Text.Json.Serialization;
using HcloudSlimApi.Utils;

namespace HcloudSlimApi.Models;

[JsonConverter(typeof(JsonSnakeCaseEnumConverter<ActionResourceType>))]
public enum ActionResourceType
{
    Undefined,
    Server,
    Image,
    Iso,
    FloatingIp,
    Network,
    Volume,
}
