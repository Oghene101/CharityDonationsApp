namespace CharityDonationsApp.Infrastructure.Configurations;

public record JwtSettings
{
    public const string Path = "Security:Jwt";
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string PrivateKey { get; init; } = string.Empty;
    public string PublicKey { get; init; } = string.Empty;
    public int ExpireMinutes { get; init; }
    public int RefreshTokenExpireDays { get; init; }
}