using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Domain.Entities;
using CharityDonationsApp.Infrastructure.Persistence.DbContexts;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Xunit.Abstractions;

namespace CharityDonationsApp.Api.IntegrationTests.AuthModule;

public class PostTests(
    ApiFactory<ApiEntryPoint> factory,
    ITestOutputHelper testOutputHelper) : IClassFixture<ApiFactory<ApiEntryPoint>>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    #region Sign Up

    [Fact]
    public async Task SignUp_ReturnsOk_WhenProfileHasBeenCreated()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = new Auth.SignUpRequest("Tester1", "Tester1", "Tester@mail.com", "Tester123@");

        var response = await _httpClient.PostAsJsonAsync("/api/auth/sign-up", request);

        var userId = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await dbContext.Users.Where(x => x.Id == userId!).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task SignUp_ReturnsBadRequest_WhenProfileAlreadyHasBeenCreated()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = new Auth.SignUpRequest("Tester1", "Tester1", "Tester@mail.com", "Tester123@");
        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode is HttpStatusCode.OK)
            .WaitAndRetryAsync(
                retryCount: 1,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (outcome, timespan, retryAttempt, context) =>
                {
                    testOutputHelper.WriteLine(
                        $"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });

        var response = await retryPolicy.ExecuteAsync(
            async _ => await _httpClient.PostAsJsonAsync("/api/auth/sign-up", request), CancellationToken.None);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await dbContext.Users.Where(x => x.Email == request.Email).ExecuteDeleteAsync();
    }

    #endregion

    #region Confirm Email

    [Fact]
    public async Task ConfirmEmail_ReturnsOk_WhenEmailHasBeenConfirmed()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = await userManager.FindByEmailAsync("Karierenogheneruemu2020@gmail.com");
        if (user!.EmailConfirmed) user.EmailConfirmed = false;
        await userManager.UpdateAsync(user);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
        var request = new Auth.ConfirmEmailRequest("Karierenogheneruemu2020@gmail.com", token);
        var response = await _httpClient.PostAsJsonAsync("/api/auth/confirm-email", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Sign In

    [Fact]
    public async Task SignIn_ReturnsOk_WhenSignInIsSuccessful()
    {
        var request = new Auth.SignInRequest("Karierenogheneruemu2020@gmail.com", "Oghene123@");

        var response = await _httpClient.PostAsJsonAsync("/api/auth/sign-in", request);

        await response.Content.ReadFromJsonAsync<ApiResponse<Auth.SignInResponse>>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SignIn_ReturnsBadRequest_WhenEmailOrPasswordIsWrong()
    {
        var request = new Auth.SignInRequest("Karierenoghene20@gmail.com", "Oghene1234@");

        var response = await _httpClient.PostAsJsonAsync("/api/auth/sign-in", request);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Refresh Token

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenTokenHasBeenRefreshed()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        var request =
            new Auth.RefreshTokenRequest(generateTokenResponse.AccessToken, generateTokenResponse.RefreshToken);
        var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh-token", request);

        await response.Content.ReadFromJsonAsync<ApiResponse<Jwt.GenerateTokenResponse>>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenAccessOrRefreshTokenIsUnauthentic()
    {
        await using var scope = factory.Services.CreateAsyncScope();

        var request = new Auth.RefreshTokenRequest("accessToken", "refreshToken");
        var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh-token", request);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Change Password

    [Fact]
    public async Task ChangePassword_ReturnsOk_WhenPasswordHasBeenChanged()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        var request =
            new Auth.ChangePasswordRequest("Oghene123@", "Oghene123@1");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);
        var response = await _httpClient.PostAsJsonAsync("/api/auth/change-password", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cleanUpRequest =
            new Auth.ChangePasswordRequest("Oghene123@1", "Oghene123@");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);
        var cleanUpResponse = await _httpClient.PostAsJsonAsync("/api/auth/change-password", cleanUpRequest);
        await cleanUpResponse.Content.ReadFromJsonAsync<ApiResponse>();
        cleanUpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_ReturnsBadRequest_WhenOldPasswordIsWrong()
    {
        await using var scope = factory.Services.CreateAsyncScope();

        var request = new Auth.ChangePasswordRequest("Oghene123@1", "Oghene123@");
        var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh-token", request);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}