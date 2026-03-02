using System.Text.Json.Serialization;
using HcloudSlimApi.Utils;

namespace HcloudSlimApi.Models;

[JsonConverter(typeof(JsonSnakeCaseEnumConverter<ActionStatus>))]
public enum ActionStatus
{
    Undefined,
    Running,
    Success,
    Error,
}
