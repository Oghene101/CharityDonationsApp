using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Features.Admin.Commands;
using CharityDonationsApp.Application.Features.Auth;
using CharityDonationsApp.Application.Features.Auth.Commands;
using CharityDonationsApp.Domain.Entities;
using RefreshToken = CharityDonationsApp.Application.Features.Auth.Commands.RefreshToken;

namespace CharityDonationsApp.Application.Extensions;

public static class Mappers
{
    #region To Entity

    public static User ToEntity(this SignUp.Command dto)
        => new()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            CreatedBy = $"{dto.FirstName} {dto.LastName}",
            UpdatedBy = $"{dto.FirstName} {dto.LastName}"
        };

    #endregion

    #region To Command

    public static SignUp.Command ToCommand(this Auth.SignUpRequest dto)
        => new(dto.FirstName, dto.LastName, dto.Email, dto.Password);

    public static ConfirmEmail.Command ToCommand(this Auth.ConfirmEmailRequest dto)
        => new(dto.Email, dto.Token);

    public static SignIn.Command ToCommand(this Auth.SignInRequest dto)
        => new(dto.Email, dto.Password);

    public static RefreshToken.Command ToCommand(this Auth.RefreshTokenRequest dto)
        => new(dto.AccessToken, dto.RefreshToken);

    public static ChangePassword.Command ToCommand(this Auth.ChangePasswordRequest dto)
        => new(dto.Email, dto.OldPassword, dto.NewPassword);

    public static SendEmailConfirmation.Command ToCommand(this Admin.SendEmailConfirmationRequest dto)
        => new(dto.Email);

    public static ForgotPassword.Command ToCommand(this Auth.ForgotPasswordRequest dto)
        => new(dto.Email);

    public static ResetPasswordRequest.Command ToCommand(this Auth.ResetPasswordRequest dto)
        => new(dto.Email, dto.Token, dto.NewPassword);

    #endregion
}