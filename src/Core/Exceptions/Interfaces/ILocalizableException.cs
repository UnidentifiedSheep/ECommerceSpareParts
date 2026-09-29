using Locan.Core.Interfaces;

namespace Exceptions.Interfaces;

public interface ILocalizableException
{
	ILocalizableMessage LocalizableMessage { get; }
}
