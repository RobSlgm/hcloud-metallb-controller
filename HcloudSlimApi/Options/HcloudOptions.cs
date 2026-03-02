using System.ComponentModel.DataAnnotations;

namespace HcloudSlimApi.Options;


public sealed class HcloudOptions
{
    public string BaseUrl { get; set; } = "https://api.hetzner.cloud";

    [Required]
    public string? ApiKey { get; set; }

    public int TimeoutSeconds { get; set; } = 10;
}
