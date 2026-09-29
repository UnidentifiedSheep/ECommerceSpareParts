using Exceptions.Base;
using Exceptions.Interfaces;
using Locan.Core.Interfaces;

namespace Analytics.Entities.Exceptions;

public class PurchaseFactNotFoundException(string id) : NotFoundException(
		null,
		new
		{
			Id = id
		}),
	ILocalizableException
{
	private static readonly ILocalizableMessage MInstance = PurchaseFactNotFoundMessage.Create();
	public ILocalizableMessage LocalizableMessage => MInstance;
}
