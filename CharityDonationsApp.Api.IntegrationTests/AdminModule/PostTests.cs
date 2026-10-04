using System.Net;
using System.Net.Http.Json;
using CharityDonationsApp.Application.Common.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CharityDonationsApp.Api.IntegrationTests.AdminModule;

[Collection(ApiCollection.Name)]
public class PostTests(ApiFactory<ApiEntryPoint> factory)
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    #region Send Email Confirmation

    [Fact]
    public async Task SendEmailConfirmation_ReturnsOk_WhenEmailConfirmationHasBeenSent()
    {
        var request = new Admin.SendEmailConfirmationRequest(TestUser.Email);

        var response = await _httpClient.PostAsJsonAsync("/api/admin/send-email-confirmation", request);

        await response.Content.ReadFromJsonAsync<ApiResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendEmailConfirmation_ReturnsNotFound_WhenEmailDoesNotExist()
    {
        var request = new Admin.SendEmailConfirmationRequest("notfound@gmail.com");

        var response = await _httpClient.PostAsJsonAsync("/api/admin/send-email-confirmation", request);

        await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendEmailConfirmation_ReturnsBadRequest_WhenEmailIsInValid()
    {
        var request = new Admin.SendEmailConfirmationRequest("invalidmail");

        var response = await _httpClient.PostAsJsonAsync("/api/admin/send-email-confirmation", request);

        await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}