using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User = CharityDonationsApp.Domain.Entities.User;

namespace CharityDonationsApp.Application.Features.Auth.Commands;

public static class ResetPassword
{
    public record Command(string Email, string Token, string NewPassword) : IRequest<Result<string>>;

    public class Handler(
        UserManager<User> userManager) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw ApiException.NotFound(new Error("Auth.Error", $"User with email '{request.Email}' not found"));

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
                throw ApiException.BadRequest(result.Errors.Select(e => new Error(e.Code, e.Description))
                    .ToArray());

            return Result.Success("Your new password has been set.");
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Authentication required");
        }
    }
}