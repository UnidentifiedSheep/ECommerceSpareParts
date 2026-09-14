using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

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

public class SaleSoftConfirmationNeededException(string confirmationCode, Dictionary<string, int> reserved)
	: LocalizedPreconditionRequiredException(
		new SoftConfirmationNeededForSaleReservationReasonMessage()
			.WithConfirmationCode(confirmationCode),
		new
		{
			ConfirmationCode = confirmationCode, Reserved = reserved
		});
