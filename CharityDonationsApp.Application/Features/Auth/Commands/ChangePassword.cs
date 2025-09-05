using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using CharityDonationsApp.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CharityDonationsApp.Application.Features.Auth.Commands;

public static class ChangePassword
{
    public record Command(
        string Email,
        string OldPassword,
        string NewPassword) : IRequest<Result<string>>;

    public class Handler(
        UserManager<User> userManager,
        IUtilityService utility,
        IUnitOfWork uOw) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var email = utility.GetSignedInUserEmail();
            if (email != request.Email) throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null) throw ApiException.NotFound(new Error("Auth.Error", $"User with '{email}' not found"));

            var isCorrectPassword = await userManager.CheckPasswordAsync(user, request.OldPassword);
            if (isCorrectPassword is false)
                throw ApiException.BadRequest(new Error("Auth.Error", "Invalid request"));

            var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (result.Succeeded is false)
                throw ApiException.BadRequest(result.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());

            user.UpdatedBy = $"{user.FirstName} {user.LastName}";
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await uOw.SaveChangesAsync(cancellationToken);

            return Result.Success("Password changed successfully");
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Old password is required");

            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Old password is required")
                .MinimumLength(8).WithMessage("Old password is not valid")
                .Matches("[A-Z]").WithMessage("Old password is not valid")
                .Matches("[a-z]").WithMessage("Old password is not valid")
                .Matches("[0-9]").WithMessage("Old password is not valid")
                .Matches("[^a-zA-Z0-9]").WithMessage("Old password is not valid");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("New password is not valid")
                .Matches("[A-Z]").WithMessage("New password is not valid")
                .Matches("[a-z]").WithMessage("New password is not valid")
                .Matches("[0-9]").WithMessage("New password is not valid")
                .Matches("[^a-zA-Z0-9]").WithMessage("New password is not valid")
                .NotEqual(x => x.OldPassword).WithMessage("New password cannot be the same as old password");

            RuleFor(x => x)
                .Must(x => x.OldPassword != x.NewPassword)
                .WithMessage("Old password and new password cannot be the same");
        }
    }
}