using FluentAssertions;
using Main.Application.Handlers.Notifications.SeeNotifications;
using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Notification;

public class SeeNotificationsTests : IntegrationTest
{
	public SeeNotificationsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<InAppNotificationsTestContext>();
	}

	private InAppNotificationsTestContext NotificationsContext =>
		GetContext<InAppNotificationsTestContext>();

	[Fact]
	public async Task SeeNotifications_MarksOnlyUnreadNotificationsOfRequestedUser()
	{
		var user = NotificationsContext.Users[0];
		var unread = NotificationsContext.Notifications
			.Where(x => x.UserId == user.Id && !x.IsSeen)
			.Take(2)
			.ToArray();
		var alreadySeen = NotificationsContext.Notifications
			.First(x => x.UserId == user.Id && x.IsSeen);
		var otherUsersNotification = NotificationsContext.Notifications
			.First(x => x.UserId != user.Id && !x.IsSeen);
		List<int> ids = new()
		{
			unread[0].Id,
			unread[1].Id,
			unread[0].Id,
			alreadySeen.Id,
			otherUsersNotification.Id,
			int.MaxValue
		};

		await Mediator.Send(new SeeNotificationsCommand(user.Id, ids), CancellationToken);

		var afterFirstCall = await Context.InAppNotifications.AsNoTracking()
			.Where(x => ids.Contains(x.Id))
			.ToArrayAsync(CancellationToken);
		afterFirstCall.Where(x => unread.Any(unreadNotification => unreadNotification.Id == x.Id))
			.Should().OnlyContain(x => x.SeenAt.HasValue);
		afterFirstCall.Single(x => x.Id == alreadySeen.Id).SeenAt.Should()
			.BeCloseTo(alreadySeen.SeenAt!.Value, TimeSpan.FromMicroseconds(1));
		afterFirstCall.Single(x => x.Id == otherUsersNotification.Id).SeenAt.Should().BeNull();

		await Mediator.Send(new SeeNotificationsCommand(user.Id, ids), CancellationToken);

		var afterSecondCall = await Context.InAppNotifications.AsNoTracking()
			.Where(x => ids.Contains(x.Id))
			.ToArrayAsync(CancellationToken);
		foreach (var notification in afterFirstCall)
		{
			var second = afterSecondCall.Single(x => x.Id == notification.Id);
			if (notification.SeenAt is { } seenAt)
				second.SeenAt.Should().BeCloseTo(seenAt, TimeSpan.FromMicroseconds(1));
			else
				second.SeenAt.Should().BeNull();
		}
	}

	[Fact]
	public async Task SeeNotifications_RejectsMoreThan100Ids()
	{
		var user = NotificationsContext.Users[0];
		var ids = Enumerable.Range(1, 101).ToArray();

		var exception = await Assert.ThrowsAsync<ValidationException>(() =>
			Mediator.Send(new SeeNotificationsCommand(user.Id, ids), CancellationToken));

		exception.Errors.Should().ContainSingle(x =>
			x.ErrorCode == NotificationsSeeTooManyMessage.Key);
	}

	[Fact]
	public async Task SeeNotifications_Accepts100Ids()
	{
		var user = NotificationsContext.Users[0];
		var unread = NotificationsContext.Notifications
			.First(x => x.UserId == user.Id && !x.IsSeen);
		var ids = Enumerable.Range(0, 99)
			.Select(offset => int.MaxValue - offset)
			.Prepend(unread.Id)
			.ToArray();

		await Mediator.Send(new SeeNotificationsCommand(user.Id, ids), CancellationToken);

		var persisted = await Context.InAppNotifications.AsNoTracking()
			.SingleAsync(x => x.Id == unread.Id, CancellationToken);
		persisted.SeenAt.Should().NotBeNull();
	}
}
