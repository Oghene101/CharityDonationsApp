using System.Security.Claims;
using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using CharityDonationsApp.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CharityDonationsApp.Application.Features.Auth.Commands;

public static class RefreshToken
{
    public record Command(string AccessToken, string RefreshToken) : IRequest<Result<Jwt.GenerateTokenResponse>>;

    public class Handler(
        IJwtService jwt,
        UserManager<User> userManager,
        IUnitOfWork uOw) : IRequestHandler<Command, Result<Jwt.GenerateTokenResponse>>
    {
        public async Task<Result<Jwt.GenerateTokenResponse>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            ClaimsPrincipal principal;
            ClaimsIdentity identity;
            try
            {
                principal = jwt.GetPrincipalFromExpiredToken(request.AccessToken);
            }
            catch (Exception)
            {
                throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));
            }

            var email = principal.FindFirstValue(ClaimTypes.Email);

            var user = await userManager.FindByEmailAsync(email!);
            if (user is null) throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));

            var isConsumed = await uOw.RefreshTokensReadRepository.ConsumeRefreshTokenAsync(
                request.RefreshToken, user.Id, $"{user.FirstName} {user.LastName}");
            if (!isConsumed) throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));

            var roles = await userManager.GetRolesAsync(user);
            var token = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user, roles), cancellationToken);

            return Result.Success(token);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token required")
                .MaximumLength(44).WithMessage("Refresh token is invalid");
        }
    }
}