using System.Security.Claims;
using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using CharityDonationsApp.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using User = CharityDonationsApp.Domain.Entities.User;

namespace CharityDonationsApp.Infrastructure.Services;

public class AuthService(
    UserManager<User> userManager,
    IOptions<EmailSettings> emailSettings,
    IOptions<ApiEndpoints> apiEndpoints,
    IOptions<AuthSettings> authSettings,
    IEmailService email,
    IUtilityService utility,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private int? _maxFailedAttempts;
    private int? _baseLockoutMinutes;
    private int? _lockoutMultiplier;
    private int? _maxLockoutMinutes;
    public int MaxFailedAttempts => _maxFailedAttempts ??= authSettings.Value.MaxFailedAttempts;
    public int BaseLockoutMinutes => _baseLockoutMinutes ??= authSettings.Value.BaseLockoutMinutes;
    public int LockoutMultiplier => _lockoutMultiplier ??= authSettings.Value.LockoutMultiplier;
    public int MaxLockoutMinutes => _maxLockoutMinutes ??= authSettings.Value.MaxLockoutMinutes;

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

    public async Task<bool> CheckPassword(Authentication.CheckPasswordRequest request)
    {
        var (user, password) = request;
        var isCorrectPassword = await userManager.CheckPasswordAsync(user, password);
        if (isCorrectPassword) return true;
        user.AccessFailedCount++;

        if (user.AccessFailedCount >= MaxFailedAttempts)
        {
            user.LockoutCount++;

            // Progressive lockout: Base * Multiplier^Count
            var lockoutMinutes = BaseLockoutMinutes *
                                 (int)Math.Pow(LockoutMultiplier, user.LockoutCount - 1);

            // Cap lockout duration
            lockoutMinutes = Math.Min(lockoutMinutes, MaxLockoutMinutes);

            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes));

            throw ApiException.BadRequest(new Error("Auth.Error",
                $"Your account has been locked for {(user.LockoutEnd! - DateTimeOffset.UtcNow).Value.TotalSeconds} seconds. Try again later"));
        }

        await userManager.UpdateAsync(user);
        throw ApiException.BadRequest(new Error("Auth.Error", "Incorrect email or password"));
    }
}