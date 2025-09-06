using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using CharityDonationsApp.Domain.Entities;
using Dapper;

namespace CharityDonationsApp.Infrastructure.Persistence.Repositories;

public class KycVerificationRepository(
    IDbConnectionFactory connectionFactory) : IKycVerificationRepository
{
    public async Task<KycVerification?> GetKycVerificationAsync(Guid userId)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  SELECT * FROM KycVerifications 
                           WHERE UserId = @userId
                  """;

        var result = await connection.QuerySingleOrDefaultAsync<KycVerification>(sql, new { userId });
        return result;
    }

    public async Task<KycVerification?> GetKycVerificationWithAddressesAsync(Guid userId)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = """
                  SELECT * FROM KycVerifications as K
                           LEFT JOIN Addresses as A ON K.Id = A.KycVerificationId
                           WHERE UserId = @userId
                  """;

        var dictionary = new Dictionary<Guid, KycVerification>();
        var result =
            await connection.QueryAsync<KycVerification, Address, KycVerification>(
                sql,
                (kyc, address) =>
                {
                    if (!dictionary.TryGetValue(kyc.Id, out var currentKyc))
                    {
                        currentKyc = kyc;
                        dictionary.Add(kyc.Id, currentKyc);
                    }

                    if (address is not null)
                    {
                        currentKyc.Addresses.Add(address);
                    }

                    return currentKyc;
                },
                new { userId },
                splitOn: "Id");
        
        return dictionary.Values.FirstOrDefault();
    }
}