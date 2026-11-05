using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public sealed class OrderNotFoundException(Guid id) : LocalizedNotFoundException(
	OrderNotFoundMessage.Instance,
	new { Id = id });

public sealed class OrderReadAccessDeniedException() : LocalizedForbiddenException(OrderReadAccessDeniedMessage.Instance);
