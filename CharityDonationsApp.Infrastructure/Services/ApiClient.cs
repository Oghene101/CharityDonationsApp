using System.Diagnostics;
using System.Net.Http.Json;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using Microsoft.Extensions.Logging;

namespace CharityDonationsApp.Infrastructure.Services;

public class ApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<ApiClient> logger) : IApiClient
{
    private static readonly string Separator = new('*', 110);

    public async Task<TResponse> GetAsync<TResponse>(string uri, string clientName = "",
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var client = ResolveHttpClient(clientName);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        ApplyHeaders(request, headers);
        return await SendAsync<TResponse>(client, request, cancellationToken);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest data, string clientName = "",
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var client = ResolveHttpClient(clientName);
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(data)
        };

        ApplyHeaders(request, headers);

        return await SendAsync<TResponse>(client, request, cancellationToken);
    }

    private async Task<TResponse> SendAsync<TResponse>(HttpClient httpClient, HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("""
                              {separator}
                              Sending HTTP {Method} request to {Uri}

                              Request Body: {@Body}
                              {separator}
                              """, Separator, request.Method, request.RequestUri,
            request.Content is null ? "nil" : await request.Content.ReadAsStringAsync(cancellationToken), Separator);

        var stopwatch = Stopwatch.StartNew();
        var response = await httpClient.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        var httpContentAsString = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("""
                            {separator}
                            Http {Method} request to {Uri} failed with {StatusCode}

                            Duration: {Duration}ms 

                            Response: {@ErrorContent}
                            {separator}
                            """, Separator, request.Method, request.RequestUri, response.StatusCode,
                stopwatch.ElapsedMilliseconds, httpContentAsString, Separator);

            response.EnsureSuccessStatusCode();
        }

        logger.LogInformation("""
                              {separator}
                              HTTP {Method} request to {Uri} was successful with {StatusCode} 

                              Duration: {Duration}ms

                              Response: {@Response}
                              {separator}
                              """, Separator, request.Method, request.RequestUri, response.StatusCode,
            stopwatch.ElapsedMilliseconds, httpContentAsString, Separator);

        var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        return result!;
    }

    private HttpClient ResolveHttpClient(string clientName)
    {
        return httpClientFactory.CreateClient(!string.IsNullOrWhiteSpace(clientName)
            ? clientName
            : string.Empty);
    }

    private void ApplyHeaders(HttpRequestMessage request, IDictionary<string, string>? headers)
    {
        if (headers == null) return;
        foreach (var (key, value) in headers)
        {
            request.Headers.Add(key, value);
        }
    }
}