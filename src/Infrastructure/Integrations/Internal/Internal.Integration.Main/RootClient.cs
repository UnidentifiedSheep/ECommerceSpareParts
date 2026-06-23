using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

public class RootClient(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor)
    : InternalClientBase(authClient, optionsMonitor), IMainClient
{
    private readonly UserNode _userNode = new(httpClient, authClient, optionsMonitor);
    private readonly ProductNode _productNode = new(httpClient, authClient, optionsMonitor);
    private readonly ProducerNode _producerNode = new(httpClient, authClient, optionsMonitor);
    private readonly CurrencyNode _currencyNode = new(httpClient, authClient, optionsMonitor);
    private readonly PurchaseNode _purchaseNode = new(httpClient, authClient, optionsMonitor);
    public IUserNode UserNode => _userNode;
    public IProductNode ProductNode => _productNode;
    public IProducerNode ProducerNode => _producerNode;
    public IPurchaseNode PurchaseNode => _purchaseNode;
    public ICurrencyNode CurrencyNode => _currencyNode;
}
