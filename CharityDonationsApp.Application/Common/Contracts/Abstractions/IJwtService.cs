using System.Security.Claims;

namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IJwtService
{
    Task<Jwt.GenerateTokenResponse> GenerateToken(Jwt.GenerateTokenRequest request,
        CancellationToken cancellationToken = default);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}