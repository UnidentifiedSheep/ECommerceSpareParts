using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Enums;

namespace Main.Entities.Storage;

public class StorageContentLock : AuditableEntity<StorageContentLock, Guid>,
	ILinqEntity<StorageContentLock, Guid>, IVersionable<uint>
{
	private StorageContentLock() { }

	private StorageContentLock(Guid id, int storageContentId, int quantity, DateTime? expiresAt)
	{
		if (id == Guid.Empty)
			throw new InvalidOperationException("Lock ID must not be empty.");
		if (storageContentId <= 0)
			throw new InvalidOperationException("Storage content ID must be greater than zero.");
		ValidateQuantity(quantity);
		if (expiresAt is { } expiration)
		{
			ValidateUtc(expiration);
			if (expiration <= CreatedAt)
				throw new InvalidOperationException("Expiration time must be in the future.");
		}

		Id = id;
		StorageContentId = storageContentId;
		Quantity = quantity;
		State = StorageContentLockState.Active;
		ExpiresAt = expiresAt;
	}

	public Guid Id { get; private set; }
	public int StorageContentId { get; private set; }
	public int Quantity { get; private set; }
	public StorageContentLockState State { get; private set; }
	public DateTime? ExpiresAt { get; private set; }
	public DateTime? ClosedAt { get; private set; }
	public uint RowVersion { get; private set; }
	public StorageContent StorageContent { get; private set; } = null!;

	public static StorageContentLock Create(
		int storageContentId, int quantity, DateTime? expiresAt = null, Guid? id = null) =>
		new(id ?? Guid.NewGuid(), storageContentId, quantity, expiresAt);

	public bool IsActiveAt(DateTime utcNow)
	{
		ValidateUtc(utcNow);
		return State == StorageContentLockState.Active && (ExpiresAt is null || utcNow < ExpiresAt.Value);
	}

	public void SetQuantity(int quantity, DateTime utcNow)
	{
		if (!IsActiveAt(utcNow))
			throw new InvalidOperationException("Only an active, unexpired lock can be changed.");
		ValidateQuantity(quantity);
		Quantity = quantity;
	}

	public void Consume(DateTime utcNow)
	{
		ValidateUtc(utcNow);
		if (State == StorageContentLockState.Consumed)
			return;
		if (!IsActiveAt(utcNow))
			throw new InvalidOperationException("Only an active, unexpired lock can be consumed.");
		Close(StorageContentLockState.Consumed, utcNow);
	}

	public void Release(DateTime utcNow) => Close(StorageContentLockState.Released, utcNow);

	public void Expire(DateTime utcNow)
	{
		ValidateUtc(utcNow);
		if (ExpiresAt is null || utcNow < ExpiresAt.Value)
			throw new InvalidOperationException("Only a lock whose expiration time has passed can expire.");
		Close(StorageContentLockState.Expired, utcNow);
	}

	private void Close(StorageContentLockState state, DateTime utcNow)
	{
		ValidateUtc(utcNow);
		if (State == state)
			return;
		if (State != StorageContentLockState.Active)
			throw new InvalidOperationException("A closed lock cannot change its state.");
		State = state;
		ClosedAt = utcNow;
	}

	private static void ValidateQuantity(int quantity)
	{
		if (quantity <= 0)
			throw new InvalidOperationException("Lock quantity must be greater than zero.");
	}

	private static void ValidateUtc(DateTime value)
	{
		if (value.Kind != DateTimeKind.Utc)
			throw new InvalidOperationException("Lock timestamps must be UTC.");
	}

	public static Expression<Func<StorageContentLock, Guid>> GetKeySelector() => value => value.Id;
	public static Expression<Func<StorageContentLock, bool>> GetEqualityExpression(Guid key) => value => value.Id == key;
	public override Guid GetId() => Id;
}
