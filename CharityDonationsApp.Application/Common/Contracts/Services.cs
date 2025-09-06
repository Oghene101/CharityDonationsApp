namespace CharityDonationsApp.Application.Common.Contracts;

public static class Jwt
{
    public record GenerateTokenRequest(Domain.Entities.User User, IList<string> Roles);

    public record GenerateTokenResponse(string AccessToken, int ExpireMinutes, string RefreshToken);
}

public static class Authentication
{
    public record CheckPasswordRequest(
        Domain.Entities.User User,
        string Password,
        int FailedLoginAttempts,
        string FailedLoginCacheKey);
}