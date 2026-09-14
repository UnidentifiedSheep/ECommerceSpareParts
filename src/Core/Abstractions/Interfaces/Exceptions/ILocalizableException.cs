using Locan.Core.Interfaces;

namespace Abstractions.Interfaces.Exceptions;

public interface ILocalizableException
{
	ILocalizableMessage LocalizableMessage { get; }
}
