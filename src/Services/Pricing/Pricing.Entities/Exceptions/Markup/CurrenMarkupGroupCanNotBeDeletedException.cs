using Exceptions.Base.Localized;

namespace Pricing.Entities.Exceptions.Markup;

public class CurrenMarkupGroupCanNotBeDeletedException() : LocalizedBadRequestException(
	CurrentMarkupGroupCanNotBeDeletedMessage.Instance);
