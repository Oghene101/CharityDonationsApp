namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IAuthService
{
    string GetSignedInUserId();
    string GetSignedInUserEmail();
    string GetSignedInUserName();
    Task SendEmailConfirmationAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);

    Task SendForgotPasswordEmailAsync(Domain.Entities.User user, CancellationToken cancellationToken = default);
}