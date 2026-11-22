using Application.Common.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration;
using Tests.Persistence.Entities;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Persistence;

public sealed class LinqRepositoryCompositeKeyTests(CombinedContainerFixture fixture)
	: CommonLayerIntegrationTest(fixture)
{
	private static readonly CompositeKeyTestKey UnknownKey = new("unknown", 1);

	private readonly CompositeKeyTestEntity[] _entities =
	[
		new("first", 1),
		new("first", 2),
		new("second", 1),
		new("second", 2)
	];

	private IRepository<CompositeKeyTestEntity, CompositeKeyTestKey> Repository
		=> Scope.ServiceProvider.GetRequiredService<IRepository<CompositeKeyTestEntity, CompositeKeyTestKey>>();

	private IReadRepository<CompositeKeyTestEntity, CompositeKeyTestKey> ReadRepository
		=> Scope.ServiceProvider.GetRequiredService<IReadRepository<CompositeKeyTestEntity, CompositeKeyTestKey>>();

	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();
		// ReSharper disable once CoVariantArrayConversion
		Context.AddRange(_entities);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();
	}

	[Fact]
	public async Task FindByIdsAsync_ReturnsOnlyExactMatches()
	{
		var result = await Repository.FindByIdsAsync(
			[_entities[0].GetId(), _entities[3].GetId(), UnknownKey],
			ct: CancellationToken);

		result.Keys.Should().BeEquivalentTo([_entities[0].GetId(), _entities[3].GetId()]);
	}

	[Fact]
	public async Task QueryByIds_ReturnsOnlyExactMatches()
	{
		var result = await ReadRepository
			.QueryByIds([_entities[0].GetId(), _entities[3].GetId(), UnknownKey])
			.Select(x => new CompositeKeyTestKey(x.Code, x.Number))
			.ToListAsync(CancellationToken);

		result.Should().BeEquivalentTo([_entities[0].GetId(), _entities[3].GetId()]);
	}

	[Fact]
	public async Task QueryByIds_EmptyKeys_ReturnsEmpty()
	{
		var result = await ReadRepository.QueryByIds([]).ToListAsync(CancellationToken);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task DeleteManyAsync_DeletesOnlyExactMatches()
	{
		await Repository.DeleteManyAsync([_entities[0].GetId(), _entities[3].GetId()], CancellationToken);

		var remaining = await Context.Set<CompositeKeyTestEntity>()
			.AsNoTracking()
			.Select(x => new CompositeKeyTestKey(x.Code, x.Number))
			.ToListAsync(CancellationToken);
		remaining.Should().BeEquivalentTo([_entities[1].GetId(), _entities[2].GetId()]);
	}
}
