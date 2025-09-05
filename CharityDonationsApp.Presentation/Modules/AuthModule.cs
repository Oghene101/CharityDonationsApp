using Carter;
using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CharityDonationsApp.Presentation.Modules;

public class AuthModule : CarterModule
{
    public AuthModule() : base("api/auth")
    {
        WithTags("Authentication");
        IncludeInOpenApi();
        RequireAuthorization();
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/sign-up", SignUpAsync)
            .WithName("SignUp")
            .WithSummary("Register a new user")
            .WithDescription("Creates a user account with credentials for authentication.")
            .AllowAnonymous();

        app.MapGet("/confirm-email", ConfirmEmailAsync)
            .WithName("ConfirmEmail")
            .WithSummary("Confirm user email")
            .WithDescription("Confirms a user's email using the confirmation token.")
            .AllowAnonymous();

        app.MapPost("/sign-in", SignInAsync)
            .WithName("SignIn")
            .WithSummary("Authenticate user")
            .WithDescription("Authenticates a user with their credentials and returns access and refresh tokens.")
            .AllowAnonymous();

        app.MapPost("/refresh-token", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .WithDescription("Exchanges a valid refresh token for a new access token a new refresh token.")
            .AllowAnonymous();

        app.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Change user password")
            .WithDescription("Allows an authenticated user to change their password.");

        app.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithSummary("Request a password reset")
            .WithDescription("Sends a password reset link to the user’s registered email.")
            .AllowAnonymous();

        app.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Reset user password")
            .WithDescription("Resets the user's password using a valid reset token from the forgot password flow.")
            .AllowAnonymous();
    }

    private static async Task<Results<Ok<ApiResponse<Guid>>, BadRequest<ValidationProblemDetails>>> SignUpAsync(
        Auth.SignUpRequest request,
        ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        Guid value = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(value);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>> ConfirmEmailAsync(
        [AsParameters] Auth.ConfirmEmailRequest request,
        ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse<Auth.SignInResponse>>, BadRequest<ValidationProblemDetails>>>
        SignInAsync(
            Auth.SignInRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        Auth.SignInResponse result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse<Jwt.GenerateTokenResponse>>, BadRequest<ValidationProblemDetails>>>
        RefreshTokenAsync(
            Auth.RefreshTokenRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        Jwt.GenerateTokenResponse result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>>
        ChangePasswordAsync(
            Auth.ChangePasswordRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>>
        ForgotPasswordAsync(
            Auth.ForgotPasswordRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>>
        ResetPasswordAsync(
            Auth.ResetPasswordRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }
}