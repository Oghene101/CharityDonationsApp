using Carter;
using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Extensions;
using CharityDonationsApp.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CharityDonationsApp.Presentation.Modules;

public class AdminModule : CarterModule
{
    public AdminModule() : base("api/admin")
    {
        WithTags("Admin");
        IncludeInOpenApi();
        RequireAuthorization();
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/send-email-confirmation", SendEmailConfirmationAsync)
            .WithName("SendEmailConfirmation")
            .WithSummary("Send email confirmation")
            .WithDescription("Allows an admin to send an email confirmation to a specified user.")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        app.MapPatch("/fast-forward-lockout", FastForwardLockOutEndAsync)
            .WithName("FastForwardLockOutEnd")
            .WithSummary("Fast forward lockout end time")
            .WithDescription("Allows an admin to fast forward a user's lockout end time.")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>>
        SendEmailConfirmationAsync(
            Admin.SendEmailConfirmationRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>>
        FastForwardLockOutEndAsync(
            Admin.FastForwardLockOutEndRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }
}