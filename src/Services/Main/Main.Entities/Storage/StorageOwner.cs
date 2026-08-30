using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Storage;

public class StorageOwner : AuditableEntity<StorageOwner, (string, Guid)>,
	ILinqEntity<StorageOwner, (string, Guid)>
{
	private StorageOwner()
	{
	}

	private StorageOwner(string storageCode, Guid userId)
	{
		StorageCode = storageCode;
		UserId = userId;
	}

	[ValidateTuple("PK")]
	[Validate]
	public string StorageCode { get; } = null!;

	[ValidateTuple("PK")]
	public Guid UserId { get; }

	public User.User User { get; private set; } = null!;

	public Storage Storage { get; private set; } = null!;

	public static Expression<Func<StorageOwner, (string, Guid)>> GetKeySelector() => x =>
		ValueTuple.Create(x.StorageCode, x.UserId);

	public static Expression<Func<StorageOwner, bool>> GetEqualityExpression((string, Guid) key) => x =>
		x.StorageCode == key.Item1 && x.UserId == key.Item2;

	public static StorageOwner Create(string storageCode, Guid ownerId) => new(storageCode, ownerId);

	public override (string, Guid) GetId() => (StorageCode, UserId);
}
