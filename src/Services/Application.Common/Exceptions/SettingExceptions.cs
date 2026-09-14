using Exceptions.Base.Localized;

namespace Application.Common.Exceptions;

public class SettingNotFound(string systemName) : LocalizedNotFoundException(
	SettingNotFoundMessage.Create(systemName),
	new
	{
		SystemName = systemName
	});
