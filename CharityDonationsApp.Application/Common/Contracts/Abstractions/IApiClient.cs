namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IApiClient
{
    Task<TResponse> GetAsync<TResponse>(string uri, string clientName = "",
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest data, string clientName = "",
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}