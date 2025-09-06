using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using CharityDonationsApp.Application.Extensions;
using CharityDonationsApp.Domain.Constants;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CharityDonationsApp.Application.Features.Auth.Commands;

public static class SignUp
{
    public record Command(
        string FirstName,
        string LastName,
        string Email,
        string Password) : IRequest<Result<Guid>>;

    public class Handler(
        UserManager<Domain.Entities.User> userManager,
        IBackgroundTaskQueue queue,
        IAuthService auth,
        ILogger<Handler> logger) : IRequestHandler<Command, Result<Guid>>
    {
        private static readonly string Separator = new('*', 110);

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = request.ToEntity();

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw ApiException.BadRequest(
                    result.Errors.Select(e => new Error(e.Code, e.Description))
                        .ToArray());

            result = await userManager.AddToRoleAsync(user, Roles.User);
            if (!result.Succeeded)
            {
                await userManager.DeleteAsync(user);
                throw ApiException.BadRequest(
                    result.Errors.Select(e => new Error(e.Code, e.Description))
                        .ToArray());
            }

            queue.QueueBackgroundWorkItem(async _ =>
            {
                try
                {
                    await auth.SendEmailConfirmationAsync(user, cancellationToken);
                }
                catch (Exception ex)
                {
                    // log and swallow, so it doesn't crash the worker
                    logger.LogError("""
                                    {Separator}
                                    Error occured while sending email confirmation

                                    Exception Message: {Message}

                                    Exception Type: {ExceptionType}
                                    {Separator}

                                    Stack Trace: {StackTrace}

                                    """, Separator, ex.Message,
                        ex.GetType().FullName ?? ex.GetType().Name, ex.StackTrace, Separator);
                }
            });

            return Result.Success(user.Id);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        }
    }
}