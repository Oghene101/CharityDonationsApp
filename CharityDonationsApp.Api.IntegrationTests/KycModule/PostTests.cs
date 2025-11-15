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

namespace CharityDonationsApp.Api.IntegrationTests.KycModule;

public class PostTests(
    ApiFactory<ApiEntryPoint> factory,
    ITestOutputHelper testOutputHelper) : IClassFixture<ApiFactory<ApiEntryPoint>>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    #region Add Bvn

    [Fact]
    public async Task AddBvn_ReturnsOk_WhenBvnHasBeenAdded()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var uOw = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var request = new Kyc.AddBvnRequest("12345678910");

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);
        var response = await _httpClient.PostAsJsonAsync("/api/kyc/add-bvn", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationAsync(user!.Id);
        kycVerification!.BvnCipher = null;
        kycVerification.BvnHash = null;
        uOw.KycVerificationsWriteRepository.Update(kycVerification, x => x.BvnCipher!, x => x.BvnHash!);
        await uOw.SaveChangesAsync();
    }

    [Fact]
    public async Task AddBvn_ReturnsBadRequest_WhenBvnHasAlreadyBeenAdded()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var uOw = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
        var request = new Kyc.AddBvnRequest("12345678910");

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);

        var response =
            await retryPolicy.ExecuteAsync(
                async ct => await _httpClient.PostAsJsonAsync("/api/kyc/add-bvn", request, cancellationToken: ct),
                CancellationToken.None);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationAsync(user!.Id);
        kycVerification!.BvnCipher = null;
        kycVerification.BvnHash = null;
        uOw.KycVerificationsWriteRepository.Update(kycVerification, x => x.BvnCipher!, x => x.BvnHash!);
        await uOw.SaveChangesAsync();
    }

    #endregion

    #region Add Nin

    [Fact]
    public async Task AddNin_ReturnsOk_WhenNinHasBeenAdded()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var uOw = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var request = new Kyc.AddNinRequest("12345678910");

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);
        var response = await _httpClient.PostAsJsonAsync("/api/kyc/add-nin", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationAsync(user!.Id);
        kycVerification!.NinCipher = null;
        kycVerification.NinHash = null;
        uOw.KycVerificationsWriteRepository.Update(kycVerification, x => x.NinCipher!, x => x.NinHash!);
        await uOw.SaveChangesAsync();
    }

    [Fact]
    public async Task AddNin_ReturnsBadRequest_WhenNinHasAlreadyBeenAdded()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var uOw = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
        var request = new Kyc.AddNinRequest("12345678910");

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);

        var response =
            await retryPolicy.ExecuteAsync(
                async ct => await _httpClient.PostAsJsonAsync("/api/kyc/add-nin", request, cancellationToken: ct),
                CancellationToken.None);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationAsync(user!.Id);
        kycVerification!.NinCipher = null;
        kycVerification.NinHash = null;
        uOw.KycVerificationsWriteRepository.Update(kycVerification, x => x.NinCipher!, x => x.NinHash!);
        await uOw.SaveChangesAsync();
    }

    #endregion

    #region Add Address

    [Fact]
    public async Task AddAddress_ReturnsOk_WhenAddressHasBeenAdded()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var uOw = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = new Kyc.AddAddressRequest(
            "8",
            "Law School",
            "Idowu Taylor",
            "Eti-Osa",
            "VI",
            "Lagos",
            "Nigeria");

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);
        var response = await _httpClient.PostAsJsonAsync("/api/kyc/add-address", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationAsync(user!.Id);
        await dbContext.Addresses.Where(x => x.KycVerificationId == kycVerification!.Id).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task AddAddress_ReturnsBadRequest_WhenTheExactAddressHasAlreadyBeenAdded()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var uOw = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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
        var request = new Kyc.AddAddressRequest(
            "8",
            "Law School",
            "Idowu Taylor",
            "Eti-Osa",
            "VI",
            "Lagos",
            "Nigeria");

        var user = await userManager.FindByEmailAsync("karierenogheneruemu2020@gmail.com");
        var roles = await userManager.GetRolesAsync(user!);
        var generateTokenResponse = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user!, roles));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, generateTokenResponse.AccessToken);

        var response =
            await retryPolicy.ExecuteAsync(
                async ct => await _httpClient.PostAsJsonAsync("/api/kyc/add-address", request, cancellationToken: ct),
                CancellationToken.None);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationAsync(user!.Id);
        await dbContext.Addresses.Where(x => x.KycVerificationId == kycVerification!.Id).ExecuteDeleteAsync();
    }

    #endregion
}