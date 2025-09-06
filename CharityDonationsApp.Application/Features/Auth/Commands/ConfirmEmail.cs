using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CharityDonationsApp.Application.Features.Auth.Commands;

public static class ConfirmEmail
{
    public record Command(string Email, string Token) : IRequest<Result<string>>;

    public class Handler(
        UserManager<Domain.Entities.User> userManager) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var email = Uri.UnescapeDataString(request.Email);
            var token = Uri.UnescapeDataString(request.Token);

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                throw ApiException.NotFound(new Error("Auth.Error", $"User with email '{email}' not found."));
            if (user.EmailConfirmed) return Result.Success("Your email has already been confirmed.");

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded is false)
                throw ApiException.BadRequest(result.Errors.Select(e => new Error(e.Code, e.Description))
                    .ToArray());

            return Result.Success("Your email has been confirmed.");
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