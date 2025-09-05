using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CharityDonationsApp.Infrastructure.Services.Integrations.Auth;

public class AlatPayAuthHeaderProvider(
    IOptions<AlatPaySettings> alatPay) : IAuthHeaderProvider
{
    private readonly string _token = alatPay.Value.ApiKey;

    public async Task<(string Name, string Value)> GetAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(("Ocp-Apim-Subscription-Key", _token));
    }
}