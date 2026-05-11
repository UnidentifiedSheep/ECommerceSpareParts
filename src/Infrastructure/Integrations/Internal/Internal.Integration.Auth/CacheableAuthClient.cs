namespace Internal.Integration.Auth;

public class CacheableAuthClient(
    HttpClient client,
    IAuthTokenCache tokenCache) : AuthClientBase(client)
{
    public override Task<string> GetAuthToken(
        string service,
        string serviceSecret,
        CancellationToken cancellationToken = default)
    {
        return tokenCache.GetOrCreateAsync(
            service,
            serviceSecret,
            ct => base.GetAuthToken(service, serviceSecret, ct),
            cancellationToken);
    }
}