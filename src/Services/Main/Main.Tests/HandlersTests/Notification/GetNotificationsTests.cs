using Abstractions.Models;
using FluentAssertions;
using Main.Application.Handlers.Notifications.GetNotifications;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Notification;

public class GetNotificationsTests : IntegrationTest
{
	public GetNotificationsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<InAppNotificationsTestContext>();
	}

	private InAppNotificationsTestContext NotificationsContext =>
		GetContext<InAppNotificationsTestContext>();

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	public async Task GetNotifications_ReturnsOnlyRequestedUsersNotificationsWithProjectedFields(int userIndex)
	{
		var user = NotificationsContext.Users[userIndex];
		var expected = NotificationsContext.Notifications
			.Where(x => x.UserId == user.Id)
			.OrderByDescending(x => x.CreateAt)
			.ToArray();

		var result = await Mediator.Send(
			new GetNotificationsQuery(user.Id, new Cursor<DateTime?>(null, 10)),
			CancellationToken);

		result.Notifications.Select(x => x.Id).Should().Equal(expected.Select(x => x.Id));
		result.Notifications.Should().OnlyContain(x => x.UserId == user.Id);
		foreach (var (actual, source) in result.Notifications.Zip(expected))
		{
			actual.Text.Should().Be(source.Text);
			actual.CreateAt.Should().BeCloseTo(source.CreateAt, TimeSpan.FromMicroseconds(1));
			if (source.SeenAt is { } seenAt)
				actual.SeenAt.Should().BeCloseTo(seenAt, TimeSpan.FromMicroseconds(1));
			else
				actual.SeenAt.Should().BeNull();
		}
		result.Notifications.Count(x => x.SeenAt.HasValue).Should().Be(2);
	}

	[Fact]
	public async Task GetNotifications_RespectsPageSizeAndCursor()
	{
		var user = NotificationsContext.Users[0];
		var firstPage = await Mediator.Send(
			new GetNotificationsQuery(user.Id, new Cursor<DateTime?>(null, 2)),
			CancellationToken);

		firstPage.Notifications.Should().HaveCount(2);
		firstPage.Notifications.Select(x => x.CreateAt).Should().BeInDescendingOrder();
		var cursor = firstPage.Notifications[^1].CreateAt;

		var secondPage = await Mediator.Send(
			new GetNotificationsQuery(
				user.Id,
				new Cursor<DateTime?>(cursor, 10)),
			CancellationToken);

		secondPage.Notifications.Should().HaveCount(3);
		secondPage.Notifications.Should().OnlyContain(x => x.CreateAt < cursor);
		secondPage.Notifications.Select(x => x.Id)
			.Should().NotIntersectWith(firstPage.Notifications.Select(x => x.Id));
	}

	[Fact]
	public async Task GetNotifications_ForUserWithoutNotifications_ReturnsEmptyList()
	{
		var user = NotificationsContext.Users[2];
		var result = await Mediator.Send(
			new GetNotificationsQuery(user.Id, new Cursor<DateTime?>(null, 10)),
			CancellationToken);

		result.Notifications.Should().BeEmpty();
	}
}
