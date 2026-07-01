using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Integrations.Client.Core;
using Integrations.Common;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Core;

public abstract class InternalClientBase(
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor) : ClientBase
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

    protected static async Task<Response<TValue>> ReadInternalResponse<TResponse, TValue>(
        HttpResponseMessage response,
        Func<TResponse, TValue> selector,
        CancellationToken cancellationToken = default)
    {
        var result = await ReadResponse<TResponse>(response, cancellationToken);

        if (!result.Success)
            return Response<TValue>.Fail(
                result.StatusCode ?? response.StatusCode,
                result.Error);

        return result.Value is null
            ? Response<TValue>.Fail(response.StatusCode, "Response body is null")
            : Response<TValue>.Ok(selector(result.Value));
    }
}
