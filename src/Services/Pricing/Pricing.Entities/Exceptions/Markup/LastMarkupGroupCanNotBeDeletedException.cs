using Exceptions.Base.Localized;

namespace Pricing.Entities.Exceptions.Markup;

public sealed class LastMarkupGroupCanNotBeDeletedException() : LocalizedBadRequestException(
	LastMarkupGroupCanNotBeDeletedMessage.Instance);
