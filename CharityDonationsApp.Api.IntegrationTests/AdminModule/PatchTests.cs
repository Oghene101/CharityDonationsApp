using System.Net;
using System.Net.Http.Json;
using CharityDonationsApp.Application.Common.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ApiResponse = CharityDonationsApp.Api.IntegrationTests.Dtos.ApiResponse;

namespace CharityDonationsApp.Api.IntegrationTests.AdminModule;

public class PatchTests(ApiFactory<ApiEntryPoint> factory) : IClassFixture<ApiFactory<ApiEntryPoint>>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    #region FastForwardLockout

    [Fact]
    public async Task FastForwardLockout_ReturnsOk_WhenLockoutTimeHasBeenBroughtForward()
    {
        var fastForwardLockOutEndRequest = new Admin.FastForwardLockOutEndRequest("karierenogheneruemu@gmail.com");

        var fastForwardLockOutEndResponse = await _httpClient.PatchAsJsonAsync("/api/admin/fast-forward-lockout",
            fastForwardLockOutEndRequest);

        await fastForwardLockOutEndResponse.Content.ReadFromJsonAsync<ApiResponse>();
        fastForwardLockOutEndResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FastForwardLockout_ReturnsNotFound_WhenEmailDoesNotExist()
    {
        var request = new Admin.FastForwardLockOutEndRequest("notfound@gmail.com");

        var response = await _httpClient.PatchAsJsonAsync("/api/admin/fast-forward-lockout", request);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FastForwardLockout_ReturnsBadRequest_WhenEmailIsInValid()
    {
        var request = new Admin.FastForwardLockOutEndRequest("R011846904");

        var response = await _httpClient.PatchAsJsonAsync("/api/admin/fast-forward-lockout", request);

        await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}