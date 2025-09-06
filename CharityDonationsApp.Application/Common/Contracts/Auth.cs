namespace CharityDonationsApp.Application.Common.Contracts;

public static class Auth
{
    public record SignUpRequest(string FirstName, string LastName, string Email, string Password);

    public record ConfirmEmailRequest(string Email, string Token);

    public record SignInRequest(string Email, string Password);

    public record SignInResponse(Guid UserId, IList<string> Roles, Jwt.GenerateTokenResponse Token);

    public record RefreshTokenRequest(string AccessToken, string RefreshToken);

    public record ChangePasswordRequest(string OldPassword, string NewPassword);

    public record ForgotPasswordRequest(string Email);

    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
}