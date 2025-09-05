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
            var principal = jwt.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.FindFirst(x => x.Type == ClaimTypes.Email)!.Value;

            var user = await userManager.FindByEmailAsync(email);
            if (user is null) throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));

            var refreshToken = await uOw.RefreshTokensReadRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken is null || refreshToken.UserId != user.Id)
                throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));

            var roles = await userManager.GetRolesAsync(user);
            var token = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user, roles), cancellationToken);

            // todo: update refresh token auditables and mark as used & revoked
            return Result.Success(token);
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token required")
                .MaximumLength(44).WithMessage("Refresh token is invalid");
        }
    }
}