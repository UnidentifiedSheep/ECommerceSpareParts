using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Internal.Integration.Core.Interfaces;

namespace Internal.Integration.Auth;

public abstract class AuthClientBase(HttpClient client) : IAuthClient
{
    public virtual async Task<string> GetAuthToken(
        string service, 
        string serviceSecret, 
        CancellationToken cancellationToken = default)
    {
        var request = new InternalLoginRequest
        {
            Service = service,
            ServiceSecret = serviceSecret
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        
        using var response = await client.PostAsync("/internal/auth/token", content, cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<InternalLoginResponse>(result)?.Token 
            ?? throw new InvalidOperationException("Unable to deserialize the response");
    }

    protected record InternalLoginRequest
    {
        [JsonPropertyName("service")]
        public required string Service { get; init; }
        
        [JsonPropertyName("secret")]
        public required string ServiceSecret { get; init; }
    }

    protected record InternalLoginResponse
    {
        [JsonPropertyName("token")]
        public required string Token { get; init; }
    }
}