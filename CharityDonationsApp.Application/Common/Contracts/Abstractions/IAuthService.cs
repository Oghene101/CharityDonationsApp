using CharityDonationsApp.Domain.Entities;

namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IAuthService
{
    Task SendEmailConfirmationAsync(User user, CancellationToken cancellationToken = default);
    Task SendForgotPasswordEmailAsync(User user, CancellationToken cancellationToken = default);

}