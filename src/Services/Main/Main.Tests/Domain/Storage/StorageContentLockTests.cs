using FluentAssertions;
using Main.Entities.Storage;
using Main.Enums;

namespace Tests.Domain.Storage;

public class StorageContentLockTests
{
	[Fact]
	public void Create_PreservesProvidedIdentityAndStartsActive()
	{
		var id = Guid.NewGuid();
		var expiresAt = DateTime.UtcNow.AddHours(1);
		var item = StorageContentLock.Create(42, 3, expiresAt, id);

		item.Id.Should().Be(id);
		item.StorageContentId.Should().Be(42);
		item.Quantity.Should().Be(3);
		item.State.Should().Be(StorageContentLockState.Active);
		item.ExpiresAt.Should().Be(expiresAt);
		item.ClosedAt.Should().BeNull();
		item.IsActiveAt(expiresAt.AddTicks(-1)).Should().BeTrue();
		item.IsActiveAt(expiresAt).Should().BeFalse();
	}

	[Fact]
	public void Create_WithoutExpiration_GeneratesIdentityAndRemainsActive()
	{
		var item = StorageContentLock.Create(42, 3);

		item.Id.Should().NotBeEmpty();
		item.ExpiresAt.Should().BeNull();
		item.IsActiveAt(DateTime.UtcNow.AddYears(1)).Should().BeTrue();
	}

	[Theory]
	[InlineData(0, 1)]
	[InlineData(-1, 1)]
	[InlineData(1, 0)]
	[InlineData(1, -1)]
	public void Create_RejectsInvalidIdentifiersAndQuantity(int storageContentId, int quantity) =>
		Assert.Throws<InvalidOperationException>(() => StorageContentLock.Create(storageContentId, quantity));

	[Fact]
	public void Create_RejectsEmptyIdentity() =>
		Assert.Throws<InvalidOperationException>(() => StorageContentLock.Create(42, 3, id: Guid.Empty));

	[Fact]
	public void Create_RejectsPastExpirationAndNonUtcExpiration()
	{
		Assert.Throws<InvalidOperationException>(() => StorageContentLock.Create(42, 3, DateTime.UtcNow.AddMinutes(-1)));
		Assert.Throws<InvalidOperationException>(() => StorageContentLock.Create(42, 3,
			DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Unspecified)));
	}

	[Fact]
	public void SetQuantity_RejectsInvalidOrExpiredChangesWithoutMutatingQuantity()
	{
		var expiresAt = DateTime.UtcNow.AddHours(1);
		var item = StorageContentLock.Create(42, 3, expiresAt);
		var now = expiresAt.AddMinutes(-1);
		item.SetQuantity(5, now);

		Assert.Throws<InvalidOperationException>(() => item.SetQuantity(0, now));
		Assert.Throws<InvalidOperationException>(() => item.SetQuantity(-1, now));
		Assert.Throws<InvalidOperationException>(() => item.SetQuantity(7, expiresAt));
		item.Quantity.Should().Be(5);
	}

	[Theory]
	[InlineData(StorageContentLockState.Consumed)]
	[InlineData(StorageContentLockState.Released)]
	[InlineData(StorageContentLockState.Expired)]
	public void Closing_IsIdempotentAndRejectsOtherTransitions(StorageContentLockState state)
	{
		var expiresAt = DateTime.UtcNow.AddHours(1);
		var item = StorageContentLock.Create(42, 3, expiresAt);
		var now = state == StorageContentLockState.Expired ? expiresAt : expiresAt.AddMinutes(-1);
		Close(item, state, now);
		Close(item, state, now.AddSeconds(1));

		item.State.Should().Be(state);
		item.ClosedAt.Should().Be(now);
		item.Quantity.Should().Be(3);
		item.IsActiveAt(now).Should().BeFalse();
		Assert.Throws<InvalidOperationException>(() => item.SetQuantity(1, now));
		foreach (var other in Enum.GetValues<StorageContentLockState>().Where(value => value != state && value != StorageContentLockState.Active))
			Assert.Throws<InvalidOperationException>(() => Close(item, other, expiresAt));
		item.State.Should().Be(state);
		item.ClosedAt.Should().Be(now);
	}

	[Fact]
	public void ExpiredLock_CannotBeConsumedBeforeBackgroundExpiration()
	{
		var expiresAt = DateTime.UtcNow.AddHours(1);
		var item = StorageContentLock.Create(42, 3, expiresAt);

		Assert.Throws<InvalidOperationException>(() => item.Consume(expiresAt));

		item.State.Should().Be(StorageContentLockState.Active);
		item.ClosedAt.Should().BeNull();
		item.Release(expiresAt);
		item.State.Should().Be(StorageContentLockState.Released);
	}

	[Fact]
	public void Expire_RejectsEarlyAndPermanentLocks()
	{
		var expiresAt = DateTime.UtcNow.AddHours(1);
		var temporary = StorageContentLock.Create(42, 3, expiresAt);
		var permanent = StorageContentLock.Create(42, 3);

		Assert.Throws<InvalidOperationException>(() => temporary.Expire(expiresAt.AddTicks(-1)));
		Assert.Throws<InvalidOperationException>(() => permanent.Expire(expiresAt));
		temporary.ClosedAt.Should().BeNull();
		permanent.ClosedAt.Should().BeNull();
	}

	[Fact]
	public void NonUtcOperation_DoesNotChangeStateOrQuantity()
	{
		var item = StorageContentLock.Create(42, 3);
		var nonUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

		Assert.Throws<InvalidOperationException>(() => item.SetQuantity(5, nonUtc));
		Assert.Throws<InvalidOperationException>(() => item.Release(nonUtc));
		Assert.Throws<InvalidOperationException>(() => item.Consume(nonUtc));
		Assert.Throws<InvalidOperationException>(() => item.Expire(nonUtc));
		item.State.Should().Be(StorageContentLockState.Active);
		item.Quantity.Should().Be(3);
		item.ClosedAt.Should().BeNull();
	}

	private static void Close(StorageContentLock item, StorageContentLockState state, DateTime now)
	{
		switch (state)
		{
			case StorageContentLockState.Consumed: item.Consume(now); break;
			case StorageContentLockState.Released: item.Release(now); break;
			case StorageContentLockState.Expired: item.Expire(now); break;
			default: throw new ArgumentOutOfRangeException(nameof(state));
		}
	}
}
