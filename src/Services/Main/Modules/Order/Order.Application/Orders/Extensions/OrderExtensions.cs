using Enums;
using Extensions;
using Main.Entities.Exceptions;
using Main.Entities.Order;
using Security.Core.Interfaces;

namespace Main.Application.Orders.Extensions;

public static class OrderExtensions
{
	public static IQueryable<Order> WhereReadableBy(this IQueryable<Order> query, IUserContext userContext)
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
