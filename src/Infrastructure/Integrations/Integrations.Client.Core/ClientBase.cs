using System.Text.Json;
using Integrations.Common;

namespace Integrations.Client.Core;

public abstract class ClientBase
{
    protected static async Task<Response<T>> ReadResponse<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Response<T>.Fail(response.StatusCode, GetError(response, json));

        if (string.IsNullOrWhiteSpace(json))
            return Response<T>.Fail(response.StatusCode, "Empty response body");

        try
        {
            var value = JsonSerializer.Deserialize<T>(json);
            return Response<T>.Ok(value);
        }
        catch (JsonException ex)
        {
            return Response<T>.Fail(response.StatusCode, ex.Message);
        }
    }
    
    protected static async Task<Response<TValue>> ReadResponse<TResponse, TValue>(
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
    
    private static string? GetError(HttpResponseMessage response, string body)
    {
        return string.IsNullOrWhiteSpace(body)
            ? response.ReasonPhrase
            : body;
    }
}
