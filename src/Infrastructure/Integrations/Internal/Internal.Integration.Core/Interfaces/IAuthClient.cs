namespace Internal.Integration.Core.Interfaces;

public interface IAuthClient
{
    Task<string> GetAuthToken(
        string service,
        string serviceSecret,
        CancellationToken cancellationToken = default);
}