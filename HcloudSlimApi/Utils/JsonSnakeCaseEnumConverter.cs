using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HcloudSlimApi.Utils;

public class JsonSnakeCaseEnumConverter<T> : JsonStringEnumConverter<T> where T : struct, Enum
{
    public JsonSnakeCaseEnumConverter() : base(JsonNamingPolicy.SnakeCaseLower) { }
}
