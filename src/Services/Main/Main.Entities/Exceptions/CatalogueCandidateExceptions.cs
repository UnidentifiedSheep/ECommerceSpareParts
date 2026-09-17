using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CatalogueCandidateNotFoundException()
	: LocalizedNotFoundException(CatalogueCandidateNotFoundMessage.Instance) { }

public class CatalogueCandidateDuplicateIdsException()
	: LocalizedBadRequestException(CatalogueCandidateDuplicateIdsNotAllowedMessage.Instance) { }
