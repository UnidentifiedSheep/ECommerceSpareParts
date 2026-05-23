using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

public class RootClient(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor)
    : InternalClientBase(authClient, optionsMonitor), IMainClient
{
    private readonly ProductNode _productNode = new(httpClient, authClient, optionsMonitor);
    private readonly ProducerNode _producerNode = new(httpClient, authClient, optionsMonitor);
    private readonly UserNode _userNode = new(httpClient, authClient, optionsMonitor);
    private readonly CurrencyNode _currencyNode = new(httpClient, authClient, optionsMonitor);
    
    
    public async Task<decimal> GetUserDiscount(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _userNode.GetUserDiscount(userId, cancellationToken);

    public async Task<decimal> GetCurrencyRate(
        int currencyId,
        CancellationToken cancellationToken = default)
        => await _currencyNode.GetCurrencyRate(currencyId, cancellationToken);

    public async Task<InternalFullProduct?> GetFullProduct(
        int productId,
        CancellationToken cancellationToken = default)
        => await _productNode.GetFullProduct(productId, cancellationToken);

    public async Task<InternalFullProducer?> GetFullProducer(
        int producerId,
        CancellationToken cancellationToken = default)
        => await _producerNode.GetFullProducer(producerId, cancellationToken);
}
