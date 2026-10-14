using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CharacteristicNotFoundException(int id) : LocalizedNotFoundException(
	CharacteristicNotFoundMessage.Instance,
	new { Id = id });

public class CharacteristicNameAlreadyExistsException() : LocalizedConflictException(
	CharacteristicNormalizedNameAlreadyExistsMessage.Instance);

public class CharacteristicInUseException(int id) : LocalizedConflictException(
	CharacteristicInUseMessage.Instance,
	new { Id = id });
