using System.Net;
using Exceptions.Interfaces;
using Locan.Core.Interfaces;

namespace Exceptions.Base.Localized;

public abstract class LocalizedForbiddenException(ILocalizableMessage message)
	: BaseValuedException(null), ILocalizableException
{
	public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
	public ILocalizableMessage LocalizableMessage { get; } = message;
}
