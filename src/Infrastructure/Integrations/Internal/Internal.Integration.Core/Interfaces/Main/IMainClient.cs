namespace Internal.Integration.Core.Interfaces.Main;

public interface IMainClient
{

	ICurrencyNode CurrencyNode { get; }

	IProductNode ProductNode { get; }

	IProducerNode ProducerNode { get; }

	IPurchaseNode PurchaseNode { get; }

	ISaleNode SaleNode { get; }
}
