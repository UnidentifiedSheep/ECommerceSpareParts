using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Application.Dtos.Users;
using Notification.Core.Entities;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class InAppNotificationDtoProjectionProvider
	: ProjectionProviderBase<InAppNotification, InAppNotificationDto>
{
	public override Expression<Func<InAppNotification, InAppNotificationDto>> Projection { get; } =
		x => new InAppNotificationDto
		{
			Id = x.Id,
			UserId = x.UserId,
			Text = x.Text,
			CreateAt = x.CreateAt,
			SeenAt = x.SeenAt
		};
}
