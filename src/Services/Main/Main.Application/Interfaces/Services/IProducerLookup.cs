using Enums;

namespace Main.Application.Interfaces.Services;

public interface IProducerLookup
{
	int? ResolveId(string producer, Supplier? supplier = null);
}
