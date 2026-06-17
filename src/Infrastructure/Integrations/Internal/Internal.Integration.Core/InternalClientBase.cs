using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Internal.Integration.Core.Interfaces;
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
}
