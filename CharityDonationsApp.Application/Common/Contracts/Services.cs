using CharityDonationsApp.Domain.Entities;

namespace CharityDonationsApp.Application.Common.Contracts;

public static class Jwt
{
    public record GenerateTokenRequest(User User, IList<string> Roles);

    public record GenerateTokenResponse(string AccessToken, int ExpireMinutes, string RefreshToken);
}