using Bogus;
using FluentAssertions;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using Tests.DataBuilders.Storage;

namespace Tests.Domain.Storage;

public sealed class StorageContentTests
{
	[Fact]
	public void Take_DecreasesCountAndRaisesMovementDelta()
	{
		var content = NewContent();

		content.Take(3, StorageMovementType.Sale, DateTime.UtcNow);

		content.Count.Should().Be(7);
		var movement = content.FlushDomainEvents().Should().ContainSingle().Which
			.Should().BeOfType<StorageContentCountUpdatedDomainEvent>().Subject;
		movement.Delta.Should().Be(-3);
		movement.NewCount.Should().Be(7);
		movement.MovementType.Should().Be(StorageMovementType.Sale);
	}

	[Fact]
	public void Take_InsufficientQuantityDoesNotChangeCountOrRaiseEvents()
	{
		var content = NewContent();

		var take = () => content.Take(11, StorageMovementType.Sale, DateTime.UtcNow);

		take.Should().Throw<NotEnoughCountOnStorageException>();
		content.Count.Should().Be(10);
		content.FlushDomainEvents().Should().BeEmpty();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Take_RejectsNonPositiveQuantity(int quantity)
	{
		var content = NewContent();

		var take = () => content.Take(quantity, StorageMovementType.Sale, DateTime.UtcNow);

		take.Should().Throw<InvalidOperationException>();
		content.Count.Should().Be(10);
		content.FlushDomainEvents().Should().BeEmpty();
	}

	private static StorageContent NewContent()
	{
		var content = new StorageContentBuilder(new Faker()).WithProductIds(1).WithCurrencyId(1).WithCount(10).Build();
		content.FlushDomainEvents();
		return content;
	}
}
