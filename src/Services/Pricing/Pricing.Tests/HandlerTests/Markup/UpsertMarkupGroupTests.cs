using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Settings;
using Contracts.Analytics;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Dtos.Markup;
using Pricing.Application.Handlers.Markup;
using Pricing.Application.Handlers.Markup.UpsertMarkupGroup;
using Pricing.Entities;
using Pricing.Entities.Settings;
using Pricing.Integration.Tests.DataBuilders.Markup;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests.HandlerTests.Markup;

public sealed class UpsertMarkupGroupTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();
		await Scope.ServiceProvider.GetRequiredService<ISettingsService>()
			.SetSetting(PricingSetting.Default, CancellationToken);
	}

	[Fact]
	public async Task NewMessage_CreatesAutomaticGroupAndRemembersMessageId()
	{
		var command = Command(Guid.NewGuid());

		var publisher = await Apply(command);

		var group = await Context.Set<MarkupGroup>().AsNoTracking()
			.Include(x => x.MarkupRanges).SingleAsync(CancellationToken);
		group.SourceMessageId.Should().Be(command.SourceMessageId);
		group.CurrencyId.Should().Be(command.MarkupGroup.CurrencyId);
		group.MarkupRanges.Should().HaveCount(command.MarkupGroup.Ranges.Count);
		publisher.PublishedMessagesOfType<MarkupRangesRefreshRequestedEvent>().Should().ContainSingle();
	}

	[Fact]
	public async Task RepeatedMessage_InAnotherScope_DoesNotCreateGroupOrPublishRefreshAgain()
	{
		var command = Command(Guid.NewGuid());
		await Apply(command);
		var groupId = await Context.Set<MarkupGroup>().Select(x => x.Id).SingleAsync(CancellationToken);

		var publisher = await Apply(command);

		(await Context.Set<MarkupGroup>().AsNoTracking().Select(x => x.Id).ToListAsync(CancellationToken))
			.Should().Equal(groupId);
		(await Context.Set<MarkupRange>().CountAsync(CancellationToken)).Should().Be(command.MarkupGroup.Ranges.Count);
		publisher.PublishedMessagesOfType<MarkupRangesRefreshRequestedEvent>().Should().BeEmpty();
	}

	[Fact]
	public async Task ConcurrentDelivery_InSeparateScopes_CreatesOnlyOneGroup()
	{
		var command = Command(Guid.NewGuid());

		await Task.WhenAll(Apply(command), Apply(command));

		(await Context.Set<MarkupGroup>().CountAsync(CancellationToken)).Should().Be(1);
		(await Context.Set<MarkupRange>().CountAsync(CancellationToken)).Should().Be(command.MarkupGroup.Ranges.Count);
	}

	[Fact]
	public async Task DifferentMessages_WithIdenticalRanges_CreateSeparateAnalysisVersions()
	{
		await Apply(Command(Guid.NewGuid()));
		await Apply(Command(Guid.NewGuid()));

		(await Context.Set<MarkupGroup>().CountAsync(CancellationToken)).Should().Be(2);
	}

	[Fact]
	public async Task FailedApplication_DoesNotRememberMessageAndCanBeRetried()
	{
		var command = Command(Guid.NewGuid());
		var invalid = command with { MarkupGroup = command.MarkupGroup with { Ranges = [] } };

		await Assert.ThrowsAsync<ValidationException>(() => Apply(invalid));

		(await Context.Set<MarkupGroup>().CountAsync(CancellationToken)).Should().Be(0);

		await Apply(command);

		(await Context.Set<MarkupGroup>().CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task DeletedOldGroup_ReleasesSourceMessageIdForRepeatedDelivery()
	{
		var first = Command(Guid.NewGuid());
		await Apply(first);
		var oldId = await Context.Set<MarkupGroup>().Select(x => x.Id).SingleAsync(CancellationToken);
		await Apply(Command(Guid.NewGuid()));
		await Mediator.Send(new DeleteMarkupCommand(oldId), CancellationToken);

		var publisher = await Apply(first);

		(await Context.Set<MarkupGroup>().CountAsync(CancellationToken)).Should().Be(2);
		(await Context.Set<MarkupGroup>().AnyAsync(x => x.Id == oldId, CancellationToken)).Should().BeFalse();
		publisher.PublishedMessagesOfType<MarkupRangesRefreshRequestedEvent>().Should().ContainSingle();
	}

	[Fact]
	public async Task EmptyMessageId_IsRejectedWithoutSavingGroup()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => Apply(Command(Guid.Empty)));

		(await Context.Set<MarkupGroup>().CountAsync(CancellationToken)).Should().Be(0);
	}

	private async Task<MessageBrokerStub> Apply(UpsertMarkupGroupCommand command)
	{
		await using var scope = Scope.ServiceProvider.CreateAsyncScope();
		var transaction = scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var publisher = (MessageBrokerStub)transaction.Context.Publisher;
		await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, CancellationToken);
		return publisher;
	}

	private static UpsertMarkupGroupCommand Command(Guid messageId) => new(new UpsertMarkupGroupDto
	{
		Name = "Auto Generated Markup",
		CurrencyId = 1,
		Ranges =
		[
			new UpsertMarkupRangeDto { RangeStart = 0m, RangeEnd = 10m, Markup = 0.2m },
			new UpsertMarkupRangeDto { RangeStart = 10m, RangeEnd = 100m, Markup = 0.1m }
		]
	}, messageId);

	[Fact]
	public async Task ManualGroups_CanBeCreatedWithoutSourceMessageId()
	{
		await Mediator.Send(new UpsertMarkupGroupCommand(Group()), CancellationToken);
		await Mediator.Send(new UpsertMarkupGroupCommand(Group()), CancellationToken);

		var groups = await Context.Set<MarkupGroup>().AsNoTracking().ToListAsync(CancellationToken);
		groups.Should().HaveCount(2);
		groups.Should().OnlyContain(x => x.SourceMessageId == null);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task EditingAutomaticGroup_PreservesSourceMessageId(bool includeSourceMessageId)
	{
		var group = await new MarkupGroupDataBuilder(Faker).AsAutoGenerated().BuildAndAddToDb(Context);
		var sourceMessageId = group.SourceMessageId;

		await Mediator.Send(new UpsertMarkupGroupCommand(Group(group.Id), includeSourceMessageId ? sourceMessageId : null), CancellationToken);

		var stored = await Context.Set<MarkupGroup>().AsNoTracking().SingleAsync(CancellationToken);
		stored.SourceMessageId.Should().Be(sourceMessageId);
		stored.Name.Should().Be("Updated markup");
	}

	[Fact]
	public async Task ManualUpdate_PublishesMarkupRefresh()
	{
		var group = await new MarkupGroupDataBuilder(Faker).BuildAndAddToDb(Context);

		var publisher = await Apply(new UpsertMarkupGroupCommand(Group(group.Id)));

		publisher.PublishedMessagesOfType<MarkupRangesRefreshRequestedEvent>().Should().ContainSingle();
	}

	private static UpsertMarkupGroupDto Group(int? id = null) => new()
	{
		Id = id,
		Name = "Updated markup",
		CurrencyId = 1,
		Ranges = [new UpsertMarkupRangeDto { RangeStart = 0m, RangeEnd = 100m, Markup = 0.2m }]
	};
}
