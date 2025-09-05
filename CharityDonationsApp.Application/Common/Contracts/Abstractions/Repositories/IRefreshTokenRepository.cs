using CharityDonationsApp.Domain.Entities;

namespace CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
}