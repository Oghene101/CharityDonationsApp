using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Infrastructure.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CharityDonationsApp.Infrastructure.Services;

public class EmailService(
    IOptions<EmailSettings> email) : IEmailService
{
    private readonly EmailSettings _email = email.Value;

    public async Task SendAsync(string recipientName, string recipientEmail, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_email.SenderName, $"{_email.SenderEmail}"));
        message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        message.Subject = subject;
        var builder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_email.Host, _email.Port, SecureSocketOptions.StartTls, cancellationToken);
        await smtp.AuthenticateAsync(_email.SenderEmail, _email.AppPassword, cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}