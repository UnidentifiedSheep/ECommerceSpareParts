using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Core;

public abstract class InternalClientBase(
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor)
{
    protected async Task<HttpRequestMessage> GetRequest(
        HttpMethod method,
        string url,
        CancellationToken ct = default)
    {
        var request = new HttpRequestMessage();
        request.Method = method;
        request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

        var currOptions = optionsMonitor.CurrentValue;
        var token = await authClient.GetAuthToken(currOptions.Service, currOptions.Secret, ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    protected static void AddLocalizationHeader(HttpRequestMessage request, string? locale)
    {
        if (locale == null) return;
        request.Headers.Add("Accept-Language", locale);
    }

    protected void SetJsonContent<TValue>(
        HttpRequestMessage request,
        TValue value)
    {
        request.Content = new StringContent(
            JsonSerializer.Serialize(value),
            Encoding.UTF8,
            "application/json");
    }

    protected static async Task<InternalResponse<T>> ReadInternalResponse<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return InternalResponse<T>.Fail(response.StatusCode, GetError(response, json));

        try
        {
            var value = JsonSerializer.Deserialize<T>(json);
            return value is null
                ? InternalResponse<T>.Fail(response.StatusCode, "Empty response body")
                : InternalResponse<T>.Ok(value);
        }
        catch (JsonException ex)
        {
            return InternalResponse<T>.Fail(response.StatusCode, ex.Message);
        }
    }

    protected static async Task<InternalResponse<TValue>> ReadInternalResponse<TResponse, TValue>(
        HttpResponseMessage response,
        Func<TResponse, TValue> selector,
        CancellationToken cancellationToken = default)
    {
        var result = await ReadInternalResponse<TResponse>(response, cancellationToken);

        if (!result.Success)
            return InternalResponse<TValue>.Fail(
                result.StatusCode ?? response.StatusCode,
                result.Error);

        return result.Value is null
            ? InternalResponse<TValue>.Fail(response.StatusCode, "Empty response body")
            : InternalResponse<TValue>.Ok(selector(result.Value));
    }

    private static string? GetError(HttpResponseMessage response, string body)
    {
        return string.IsNullOrWhiteSpace(body)
            ? response.ReasonPhrase
            : body;
    }
}
