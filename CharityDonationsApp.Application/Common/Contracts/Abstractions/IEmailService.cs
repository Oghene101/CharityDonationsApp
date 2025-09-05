namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IEmailService
{
    Task SendAsync(string recipientName, string recipientEmail, string subject, string body,
        CancellationToken cancellationToken = default);
}