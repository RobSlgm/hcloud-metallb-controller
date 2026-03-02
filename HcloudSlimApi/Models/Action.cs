using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HcloudSlimApi.Models;

public sealed class Action
{
    public long Id { get; set; }
    public string? Command { get; set; }
    public ActionStatus Status { get; set; }
    public DateTimeOffset? Started { get; set; }
    public DateTimeOffset? Finished { get; set; }
    public int Progress { get; set; }
    public List<ActionResource>? Resources { get; set; }
    public ActionError? Error { get; set; }

    [JsonIgnore]
    public bool IsSuccessful
    {
        get
        {
            return Status == ActionStatus.Success;
        }
    }

    [JsonIgnore]
    public bool IsFinal
    {
        get
        {
            return new[] { ActionStatus.Success, ActionStatus.Error }.Contains(Status);
        }
    }
}
