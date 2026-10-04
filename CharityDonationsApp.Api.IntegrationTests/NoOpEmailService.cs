using CharityDonationsApp.Application.Common.Contracts.Abstractions;

namespace CharityDonationsApp.Api.IntegrationTests;

public class NoOpEmailService : IEmailService
{
    public Task SendAsync(string recipientName, string recipientEmail, string subject, string body,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
