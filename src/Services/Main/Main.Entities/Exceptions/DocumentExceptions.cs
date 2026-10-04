using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class DocumentGenerationRequestNotFoundException(Guid requestId) : LocalizedNotFoundException(
	DocumentGenerationRequestNotFoundMessage.Instance,
	new { RequestId = requestId });

public class DocumentNotReadyException(Guid requestId) : LocalizedConflictException(
	DocumentNotReadyMessage.Instance,
	new { RequestId = requestId });

public class DocumentGenerationFailedException(Guid requestId) : LocalizedConflictException(
	DocumentGenerationFailedMessage.Instance,
	new { RequestId = requestId });

public class DocumentGenerationCancelledException(Guid requestId) : LocalizedConflictException(
	DocumentGenerationCancelledMessage.Instance,
	new { RequestId = requestId });
