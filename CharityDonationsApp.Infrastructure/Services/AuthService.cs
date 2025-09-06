using System.Security.Claims;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Domain.Entities;
using CharityDonationsApp.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CharityDonationsApp.Infrastructure.Services;

public class AuthService(
    UserManager<User> userManager,
    IOptions<EmailSettings> emailSettings,
    IOptions<ApiEndpoints> apiEndpoints,
    IEmailService email,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly ApiEndpoints _apiEndpoints = apiEndpoints.Value;
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;


    # region Claims

    public string GetSignedInUserId() => User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                            throw new InvalidOperationException("JWT missing 'sub' claim.");

    public string GetSignedInUserEmail() => User?.FindFirstValue(ClaimTypes.Email) ??
                                            throw new InvalidOperationException("JWT missing 'email' claim.");
    public string GetSignedInUserName() => User?.FindFirstValue(ClaimTypes.Name) ??
                                            throw new InvalidOperationException("JWT missing 'name' claim.");

    #endregion

    #region Email

    public async Task SendEmailConfirmationAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email!);
        var confirmationLink =
            $"{_emailSettings.ConfirmEmailEndpoint}email={encodedEmail}&token={encodedToken}";

        var subject = "Confirm your email";
        var body = $"""
                    
                           <p>Hello {user.FirstName},</p>
                           <p>Please confirm your email by clicking the link below:</p>
                           <p><a href='{confirmationLink}'>Confirm Email</a></p>
                           <p>This link will expire shortly for your security.</p>
                    """;

        await email.SendAsync(user.FirstName, user.Email!, subject, body, cancellationToken);
    }

    public async Task SendForgotPasswordEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email!);
        var passwordResetLink =
            $"{_apiEndpoints.ResetPasswordEndpoint}email={encodedEmail}&token={encodedToken}";

        var subject = "Reset your password";
        var body = $"""
                    
                           <p>Hello {user.FirstName},</p>
                           <p>Please reset your password by clicking the link below:</p>
                           <p><a href='{passwordResetLink}'>Reset Password</a></p>
                           <p>This link will expire shortly for your security.</p>
                    """;

        await email.SendAsync(user.FirstName, user.Email!, subject, body, cancellationToken);
    }

    #endregion
}