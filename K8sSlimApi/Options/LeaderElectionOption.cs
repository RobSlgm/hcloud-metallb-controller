namespace K8sSlimApi.Options;

public sealed class LeaderElectionOption
{
    public int LeaseDurationSeconds { get; set; } = 15;
    public int RenewDeadlineSeconds { get; set; } = 10;
    public int RetryPeriodSeconds { get; set; } = 2;
}
