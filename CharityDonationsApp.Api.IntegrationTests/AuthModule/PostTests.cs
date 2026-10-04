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

[Collection(ApiCollection.Name)]
public class PostTests(
    ApiFactory<ApiEntryPoint> factory,
    ITestOutputHelper testOutputHelper)
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

        var user = await userManager.FindByEmailAsync(TestUser.Email);
        if (user!.EmailConfirmed) user.EmailConfirmed = false;
        await userManager.UpdateAsync(user);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
        var request = new Auth.ConfirmEmailRequest(TestUser.Email, token);
        var response = await _httpClient.PostAsJsonAsync("/api/auth/confirm-email", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Sign In

    [Fact]
    public async Task SignIn_ReturnsOk_WhenSignInIsSuccessful()
    {
        var request = new Auth.SignInRequest(TestUser.Email, TestUser.Password);

        var response = await _httpClient.PostAsJsonAsync("/api/auth/sign-in", request);

        await response.Content.ReadFromJsonAsync<ApiResponse<Auth.SignInResponse>>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SignIn_ReturnsBadRequest_WhenEmailOrPasswordIsWrong()
    {
        var request = new Auth.SignInRequest("unknown@example.com", "Wrong1234@");

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

        var user = await userManager.FindByEmailAsync(TestUser.Email);
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        var request =
            new Auth.RefreshTokenRequest(generateTokenResponse.AccessToken, generateTokenResponse.RefreshToken);
        var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh-token", request);

        await response.Content.ReadFromJsonAsync<ApiResponse<Jwt.GenerateTokenResponse>>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenHasAlreadyBeenUsed()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var user = await userManager.FindByEmailAsync(TestUser.Email);
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        var request =
            new Auth.RefreshTokenRequest(generateTokenResponse.AccessToken, generateTokenResponse.RefreshToken);
        var firstResponse = await _httpClient.PostAsJsonAsync("/api/auth/refresh-token", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh-token", request);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOkOnlyOnce_WhenTheSameRefreshTokenIsUsedConcurrently()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
        const int concurrentRequests = 10;

        var user = await userManager.FindByEmailAsync(TestUser.Email);
        var roles = await userManager.GetRolesAsync(user!);

        // How much the requests overlap depends on timing, so repeat to give a regression several chances to show
        for (var round = 1; round <= 5; round++)
        {
            var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
            var request =
                new Auth.RefreshTokenRequest(generateTokenResponse.AccessToken, generateTokenResponse.RefreshToken);
            var responses = await Task.WhenAll(Enumerable.Range(0, concurrentRequests)
                .Select(_ => _httpClient.PostAsJsonAsync("/api/auth/refresh-token", request)));

            testOutputHelper.WriteLine(
                $"Round {round}: {string.Join(", ", responses.Select(x => (int)x.StatusCode))}");
            responses.Count(x => x.StatusCode is HttpStatusCode.OK).Should().Be(1);
            responses.Count(x => x.StatusCode is HttpStatusCode.Unauthorized).Should().Be(concurrentRequests - 1);
        }
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

        var user = await userManager.FindByEmailAsync(TestUser.Email);
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        var request =
            new Auth.ChangePasswordRequest(TestUser.Password, TestUser.NewPassword);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);
        var response = await _httpClient.PostAsJsonAsync("/api/auth/change-password", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cleanUpRequest =
            new Auth.ChangePasswordRequest(TestUser.NewPassword, TestUser.Password);
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
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var user = await userManager.FindByEmailAsync(TestUser.Email);
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        var request = new Auth.ChangePasswordRequest(TestUser.NewPassword, TestUser.Password);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);
        var response = await _httpClient.PostAsJsonAsync("/api/auth/change-password", request);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problemDetails!.Title.Should().Be("Bad Request"); // rejected by the handler, not by request validation
    }

    #endregion
}