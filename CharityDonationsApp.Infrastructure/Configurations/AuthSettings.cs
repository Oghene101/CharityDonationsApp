namespace CharityDonationsApp.Infrastructure.Configurations;

public record AuthSettings
{
    public const string Path = "Security:Authentication";
    public int MaxFailedAttempts { get; set; }
    public int BaseLockoutMinutes { get; set; }
    public int LockoutMultiplier { get; set; }
    public int MaxLockoutMinutes { get; set; }
}