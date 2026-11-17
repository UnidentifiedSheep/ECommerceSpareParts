using Exceptions.Base.Localized;

namespace Pricing.Entities.Exceptions;

public class PriceApplierNotFoundException(string systemName) : LocalizedNotFoundException(
	PriceApplierNotFoundMessage.Instance,
	new
	{
		SystemName = systemName
	});

public class LocalPriceApplierCannotBeDeletedException(string systemName) : LocalizedBadRequestException(
	PriceApplierLocalCannotBeDeletedMessage.Instance,
	new
	{
		SystemName = systemName
	});
