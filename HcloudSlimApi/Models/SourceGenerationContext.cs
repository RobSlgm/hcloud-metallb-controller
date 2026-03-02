
using System.Text.Json.Serialization;

namespace HcloudSlimApi.Models;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(Action))]
[JsonSerializable(typeof(ActionError))]
[JsonSerializable(typeof(ActionList))]
[JsonSerializable(typeof(ActionRecord))]
[JsonSerializable(typeof(ActionResource))]
[JsonSerializable(typeof(AssignAliasIpRequest))]
[JsonSerializable(typeof(AssignFloatingIpRequest))]
[JsonSerializable(typeof(FloatingIp))]
[JsonSerializable(typeof(FloatingIpList))]
[JsonSerializable(typeof(FloatingIpRecord))]
[JsonSerializable(typeof(PrivateNet))]
[JsonSerializable(typeof(PublicNet))]
[JsonSerializable(typeof(Server))]
[JsonSerializable(typeof(ServerList))]
[JsonSerializable(typeof(ServerRecord))]
[JsonSerializable(typeof(Network))]
[JsonSerializable(typeof(NetworkList))]
[JsonSerializable(typeof(NetworkRecord))]
public sealed partial class SourceGenerationContext : JsonSerializerContext { }
