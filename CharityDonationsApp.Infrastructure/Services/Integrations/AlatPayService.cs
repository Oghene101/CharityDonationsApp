using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Contracts.Integrations;
using CharityDonationsApp.Domain.Constants;
using CharityDonationsApp.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CharityDonationsApp.Infrastructure.Services.Integrations;

public class AlatPayService(
    IApiClient apiClient,
    IOptions<AlatPaySettings> alatPay,
    ILogger<AlatPayService> logger)
{
    private readonly AlatPaySettings _alatPay = alatPay.Value;

    public async Task<AlatPayApiResponse<AlatPayInitializeCardData>> InitializeCardAsync(
        AlatPayInitializeCardRequest request, CancellationToken ct = default)
    {
        var endpoint = _alatPay.InitializeCardEndpoint;

        var result = await apiClient
            .PostAsync<AlatPayInitializeCardRequest, AlatPayApiResponse<AlatPayInitializeCardData>>(endpoint,
                request, HttpClientNames.AlatPayClient, cancellationToken: ct);

        return result;
    }

    public async Task<AlatPayApiResponse<AlatPayAuthenticateCardData>> AuthenticateCardAsync(
        AlatPayAuthenticateCardRequest request, CancellationToken ct = default)
    {
        var endpoint = _alatPay.InitializeCardEndpoint;

        var result = await apiClient
            .PostAsync<AlatPayAuthenticateCardRequest, AlatPayApiResponse<AlatPayAuthenticateCardData>>(
                endpoint, request, HttpClientNames.AlatPayClient, cancellationToken: ct);

        return result;
    }
}