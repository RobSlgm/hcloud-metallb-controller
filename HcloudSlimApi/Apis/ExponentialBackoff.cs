using System;

namespace HcloudSlimApi.Apis;

sealed class ExponentialBackoff(int baseSeconds, int capSeconds, double multiplier, bool useJitter = false)
{
    // Use same logic as hcloud-go
    public TimeSpan NextDelay(int retries)
    {
        var basis = baseSeconds * 100000000;
        var cap = capSeconds * 100000000;
        var backoff = basis * Math.Pow(multiplier, retries);
        backoff = Math.Min(cap, backoff);
        if (useJitter)
        {
            backoff = ((backoff - basis) * Random.Shared.NextDouble()) + basis;
        }
        return TimeSpan.FromMicroseconds(backoff / 10);
    }
}
