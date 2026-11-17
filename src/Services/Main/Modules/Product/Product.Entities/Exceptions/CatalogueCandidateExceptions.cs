using Exceptions.Base.Localized;

namespace Main.Entities.Product.Exceptions;

public class CatalogueCandidateNotFoundException()
	: LocalizedNotFoundException(CatalogueCandidateNotFoundMessage.Instance)
{
}

public class CatalogueCandidateDuplicateIdsException()
	: LocalizedBadRequestException(CatalogueCandidateDuplicateIdsNotAllowedMessage.Instance)
{
}
