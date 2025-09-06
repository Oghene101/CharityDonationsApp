using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Features.Admin.Commands;
using CharityDonationsApp.Application.Features.Auth;
using CharityDonationsApp.Application.Features.Auth.Commands;
using CharityDonationsApp.Application.Features.User.Commands;
using CharityDonationsApp.Domain.Entities;
using RefreshToken = CharityDonationsApp.Application.Features.Auth.Commands.RefreshToken;
using User = CharityDonationsApp.Domain.Entities.User;

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

    public static Address ToEntity(this AddAddress.Command dto)
        => new()
        {
            HouseNumber = dto.HouseNumber,
            Landmark = dto.Landmark,
            Street = dto.Street,
            Lga = dto.Lga,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
        };

    #endregion

    #region To Command

    #region Auth

    public static SignUp.Command ToCommand(this Auth.SignUpRequest dto)
        => new(dto.FirstName, dto.LastName, dto.Email, dto.Password);

    public static ConfirmEmail.Command ToCommand(this Auth.ConfirmEmailRequest dto)
        => new(dto.Email, dto.Token);

    public static SignIn.Command ToCommand(this Auth.SignInRequest dto)
        => new(dto.Email, dto.Password);

    public static RefreshToken.Command ToCommand(this Auth.RefreshTokenRequest dto)
        => new(dto.AccessToken, dto.RefreshToken);

    public static ChangePassword.Command ToCommand(this Auth.ChangePasswordRequest dto)
        => new(dto.OldPassword, dto.NewPassword);

    public static ForgotPassword.Command ToCommand(this Auth.ForgotPasswordRequest dto)
        => new(dto.Email);

    public static ResetPasswordRequest.Command ToCommand(this Auth.ResetPasswordRequest dto)
        => new(dto.Email, dto.Token, dto.NewPassword);

    #endregion

    #region User

    public static AddBvn.Command ToCommand(this Common.Contracts.User.AddBvnRequest dto)
        => new(dto.Bvn);

    public static AddNin.Command ToCommand(this Common.Contracts.User.AddNinRequest dto)
        => new(dto.Nin);

    public static AddAddress.Command ToCommand(this Common.Contracts.User.AddAddressRequest dto)
        => new(dto.HouseNumber, dto.Landmark, dto.Street, dto.Lga, dto.City, dto.State, dto.Country);

    #endregion

    #region Admin

    public static SendEmailConfirmation.Command ToCommand(this Admin.SendEmailConfirmationRequest dto)
        => new(dto.Email);

    public static FastForwardLockoutEnd.Command ToCommand(this Admin.FastForwardLockOutEndRequest dto)
        => new(dto.Email);

    #endregion

    #endregion
}