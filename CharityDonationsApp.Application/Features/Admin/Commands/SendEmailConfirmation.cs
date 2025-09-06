using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CharityDonationsApp.Application.Features.Admin.Commands;

public static class SendEmailConfirmation
{
    public record Command(string Email) : IRequest<Result<string>>;

    public class Handler(
        UserManager<Domain.Entities.User> userManager,
        IAuthService auth) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                throw ApiException.NotFound(
                    new Error("Admin.Error", $"User with email '{request.Email}' not found."));

            await auth.SendEmailConfirmationAsync(user, cancellationToken);
            return Result.Success($"Email confirmation email sent to {user.Email}.");
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");
        }
    }
}