using Enums;

namespace Main.Modules.Product.Application.Interfaces.Services;

public interface IProducerLookup
{
	int? ResolveId(string producer, Supplier? supplier = null);
}
