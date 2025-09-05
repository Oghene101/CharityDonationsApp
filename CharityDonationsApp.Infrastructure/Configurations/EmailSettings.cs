namespace CharityDonationsApp.Infrastructure.Configurations;

public record EmailSettings
{
    public const string Path = "Email";
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
    public string AppPassword { get; init; } = string.Empty;
    public string ConfirmEmailEndpoint { get; init; } = string.Empty;
}