using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CharityDonationsApp.Application.Features.Auth.Commands;

public static class SignIn
{
    public record Command(string Email, string Password) : IRequest<Result<Common.Contracts.Auth.SignInResponse>>;

    public class Handler(
        UserManager<Domain.Entities.User> userManager,
        IJwtService jwt,
        IUtilityService utility) : IRequestHandler<Command, Result<Common.Contracts.Auth.SignInResponse>>
    {
        private const string SignInTokenCacheKey = "UserAuthToken";
        private const string UserRolesCacheKey = "UserRoles";

        public async Task<Result<Common.Contracts.Auth.SignInResponse>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null) throw ApiException.BadRequest(new Error("Auth.Error", "Incorrect email or password"));

            var isCorrectPassword = await userManager.CheckPasswordAsync(user, request.Password);
            if (isCorrectPassword is false)
                throw ApiException.BadRequest(new Error("Auth.Error", "Incorrect email or password"));

            var signInTokenCacheKey = user.Email + SignInTokenCacheKey;
            var userRolesCacheKey = user.Email + UserRolesCacheKey;
            if (!utility.TryGetInMemoryCacheValue(signInTokenCacheKey,
                    out Jwt.GenerateTokenResponse? generateTokenResponse) ||
                !utility.TryGetInMemoryCacheValue(userRolesCacheKey, out IList<string>? roles))
            {
                roles = await userManager.GetRolesAsync(user);

                generateTokenResponse =
                    await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user, roles), cancellationToken);

                var absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(generateTokenResponse.ExpireMinutes);
                utility.SetInMemoryCache(signInTokenCacheKey, generateTokenResponse, absoluteExpirationRelativeToNow);
                utility.SetInMemoryCache(userRolesCacheKey, roles, absoluteExpirationRelativeToNow);
            }

            return Result.Success(new Common.Contracts.Auth.SignInResponse(user.Id, roles!, generateTokenResponse!));
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password is not valid")
                .Matches("[A-Z]").WithMessage("Password is not valid")
                .Matches("[a-z]").WithMessage("Password is not valid")
                .Matches("[0-9]").WithMessage("Password is not valid")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password is not valid");
        }
    }
}