namespace CharityDonationsApp.Application.Common.Contracts;

public static class Jwt
{
    public record GenerateTokenRequest(Domain.Entities.User User, IList<string> Roles);

    public record GenerateTokenResponse(string AccessToken, int ExpireMinutes, string RefreshToken);
}