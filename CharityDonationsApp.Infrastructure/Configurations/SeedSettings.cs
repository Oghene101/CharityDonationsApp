namespace CharityDonationsApp.Infrastructure.Configurations;

public record SeedSettings
{
    public const string Path = "Seed";
    public SeedAccount Admin { get; init; } = new();
    public SeedAccount User { get; init; } = new();
}

public record SeedAccount
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
