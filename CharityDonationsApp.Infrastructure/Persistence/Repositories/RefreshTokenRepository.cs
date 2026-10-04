using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using CharityDonationsApp.Domain.Entities;
using Dapper;

namespace CharityDonationsApp.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(
    IDbConnectionFactory connectionFactory) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  SELECT * FROM RefreshTokens 
                           WHERE Token = @token
                           AND IsRevoked = 0
                           AND IsUsed = 0
                           AND ExpiresAt > SWITCHOFFSET(SYSDATETIMEOFFSET(), '+00:00')
                  """;

        var result = await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { token });
        return result;
    }

    // Checks and marks the token in a single statement, so concurrent requests cannot both use it
    public async Task<bool> ConsumeRefreshTokenAsync(string token, Guid userId, string updatedBy)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  UPDATE RefreshTokens
                           SET IsUsed = 1,
                               IsRevoked = 1,
                               UpdatedAt = SWITCHOFFSET(SYSDATETIMEOFFSET(), '+00:00'),
                               UpdatedBy = @updatedBy
                           WHERE Token = @token
                           AND UserId = @userId
                           AND IsRevoked = 0
                           AND IsUsed = 0
                           AND ExpiresAt > SWITCHOFFSET(SYSDATETIMEOFFSET(), '+00:00')
                  """;

        var affectedRows = await connection.ExecuteAsync(sql, new { token, userId, updatedBy });
        return affectedRows == 1;
    }
}