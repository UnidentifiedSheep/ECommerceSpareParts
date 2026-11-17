using Exceptions.Base.Localized;

namespace Main.Entities.Sale.Exceptions;

public class SaleContentNotFoundException(int id) : LocalizedBadRequestException(
	SaleContentNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class SaleNotFoundException(Guid id) : LocalizedNotFoundException(
	SaleNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class SaleContentProductMismatchException(int id) : LocalizedBadRequestException(
	SaleContentProductMismatchMessage.Instance,
	new
	{
		Id = id
	});
