using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using CharityDonationsApp.Domain.Entities;
using Dapper;

namespace CharityDonationsApp.Infrastructure.Persistence.Repositories;

public class AddressRepository(
    IDbConnectionFactory connectionFactory) : IAddressRepository
{
    public async Task<IEnumerable<Address>> GetAddressesAsync(Guid kycVerificationId)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  SELECT * FROM Addresses 
                           WHERE KycVerificationId = @kycVerificationId
                  """;

        var result = await connection.QueryAsync<Address>(sql, new { kycVerificationId });
        return result;
    }

    public async Task<IEnumerable<Address>> GetMostRecentAddressAsync(Guid kycVerificationId)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  SELECT top (1) * FROM Addresses 
                           WHERE KycVerificationId = @kycVerificationId
                           ORDER BY CreatedAt DESC
                  """;

        var result = await connection.QueryAsync<Address>(sql, new { kycVerificationId });
        return result;
    }

    public async Task<bool> AddressExistsAsync(Guid kycVerificationId, string houseNumber,
        string street, string city, string state, string country)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  SELECT 1 FROM Addresses 
                           WHERE KycVerificationId = @kycVerificationId
                           AND HouseNumber = @houseNumber
                           AND Street = @street
                           AND City = @city
                           AND State = @state
                           AND Country = @country
                  """;

        var result = await connection.ExecuteScalarAsync<int?>(sql,
            new { kycVerificationId, houseNumber, street, city, state, country });

        return result.HasValue;
    }
}