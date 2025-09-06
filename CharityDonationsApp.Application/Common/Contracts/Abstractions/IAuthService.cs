namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IAuthService
{
    int MaxFailedAttempts { get; }
    int BaseLockoutMinutes { get; }
    int LockoutMultiplier { get; }
    int MaxLockoutMinutes { get; }

    string GetSignedInUserId();
    string GetSignedInUserEmail();
    string GetSignedInUserName();
    Task SendEmailConfirmationAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);
    Task SendForgotPasswordEmailAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);
    Task<bool> CheckPassword(Authentication.CheckPasswordRequest request);
}