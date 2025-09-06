using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CharityDonationsApp.Application.Features.Admin.Commands;

public static class FastForwardLockoutEnd
{
    public record Command(string Email) : IRequest<Result<string>>;

    public class Handler(
        UserManager<Domain.Entities.User> userManager) : IRequestHandler<Command, Result<string>>
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
}