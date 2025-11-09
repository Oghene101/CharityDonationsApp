using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User = CharityDonationsApp.Domain.Entities.User;

namespace CharityDonationsApp.Application.Features.Admin.Commands;

public static class FastForwardLockoutEnd
{
    public record Command(string Email) : IRequest<Result<string>>;

    public class Handler(
        UserManager<User> userManager) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                throw ApiException.NotFound(new Error("Admin.Error", $"User with '{request.Email}' not found"));

            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            if (result.Succeeded is false)
                throw ApiException.BadRequest(result.Errors.Select(e => new Error(e.Code, e.Description))
                    .ToArray());
            return Result.Success("Lockout end has been successfully fast-forwarded");
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