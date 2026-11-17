using Exceptions.Base.Localized;
using Main.Entities;

namespace Application.Common.Exceptions;

public class SettingNotFound(string systemName) : LocalizedNotFoundException(
	SettingNotFoundMessage.Create(systemName),
	new
	{
		SystemName = systemName
	});
