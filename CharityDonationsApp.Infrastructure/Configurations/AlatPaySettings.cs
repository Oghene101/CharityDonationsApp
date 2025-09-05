namespace CharityDonationsApp.Infrastructure.Configurations;

public record AlatPaySettings
{
    public const string Path = "Integrations:AlatPay";
    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string InitializeCardEndpoint { get; init; } = string.Empty;
    public string AuthenticateCardEndpoint { get; init; } = string.Empty;
}