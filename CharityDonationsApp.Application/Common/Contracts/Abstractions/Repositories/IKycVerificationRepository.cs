using CharityDonationsApp.Domain.Entities;

namespace CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;

public interface IKycVerificationRepository
{
    Task<KycVerification?> GetKycVerificationAsync(Guid userId);
    Task<KycVerification?> GetKycVerificationWithAddressesAsync(Guid userId);
}