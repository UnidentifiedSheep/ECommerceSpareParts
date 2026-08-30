using Abstractions.Models.Options;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

public class RootClient(
	HttpClient httpClient,
	IAuthClient authClient,
	IOptionsMonitor<InternalServiceCredentials> optionsMonitor,
	ProjectJsonOptions jsonOptions) : InternalClientBase(
		authClient,
		optionsMonitor,
		jsonOptions),
	IMainClient
{
	private readonly CurrencyNode _currencyNode = new(
		httpClient,
		authClient,
		optionsMonitor,
		jsonOptions);

	private readonly ProducerNode _producerNode = new(
		httpClient,
		authClient,
		optionsMonitor,
		jsonOptions);

	private readonly ProductNode _productNode = new(
		httpClient,
		authClient,
		optionsMonitor,
		jsonOptions);

	private readonly PurchaseNode _purchaseNode = new(
		httpClient,
		authClient,
		optionsMonitor,
		jsonOptions);

	private readonly SaleNode _saleNode = new(
		httpClient,
		authClient,
		optionsMonitor,
		jsonOptions);

	private readonly UserNode _userNode = new(
		httpClient,
		authClient,
		optionsMonitor,
		jsonOptions);

	public IUserNode UserNode => _userNode;

	public IProductNode ProductNode => _productNode;

	public IProducerNode ProducerNode => _producerNode;

	public IPurchaseNode PurchaseNode => _purchaseNode;

	public ISaleNode SaleNode => _saleNode;

	public ICurrencyNode CurrencyNode => _currencyNode;
}
