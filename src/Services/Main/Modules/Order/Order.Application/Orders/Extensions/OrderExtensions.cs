using Enums;
using Extensions;
using Main.Entities.Order.Exceptions;
using Security.Core.Interfaces;

namespace Main.Modules.Order.Application.Orders.Extensions;

public static class OrderExtensions
{
	public static IQueryable<Entities.Order.Order.Order> WhereReadableBy(
		this IQueryable<Entities.Order.Order.Order> query,
		IUserContext userContext)
	{
		if (!userContext.IsAuthenticated || userContext.UserIdOrNull is not { } userId)
			throw new OrderReadAccessDeniedException();
		if (userContext.Permissions.Contains(PermissionCodes.ORDERS_GET.ToString().ToNormalizedPermission()))
			return query;
		if (!userContext.Permissions.Contains(
				PermissionCodes.ORDERS_GET_OWN.ToString().ToNormalizedPermission()))
			throw new OrderReadAccessDeniedException();
		return query.Where(order => order.UserId == userId);
	}
}
