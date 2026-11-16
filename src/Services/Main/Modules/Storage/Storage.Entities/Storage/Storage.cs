using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.Storage.Enums;

namespace Main.Entities.Storage.Storage;

public class Storage : AuditableEntity<Storage, string>, ILinqEntity<Storage, string>
{
	private Storage()
	{
	}

	private Storage(string code, StorageType type)
	{
		SetCode(code);
		SetType(type);
	}

	[Validate]
	public string Code { get; private set; } = null!;

	public string? Description { get; private set; }

	public string? Location { get; private set; }

	public StorageType Type { get; private set; }

	public static Expression<Func<Storage, string>> GetKeySelector() => x => x.Code;

	public static Expression<Func<Storage, bool>> GetEqualityExpression(string key) => x => x.Code == key;

	public static Storage Create(string code, StorageType type) => new(code, type);

	private void SetCode(string code)
		=> Code = code
			.Trim()
			.EnsureNotNullOrEmpty(StorageCodeNotEmptyMessage.Instance)
			.EnsureMinLength(6, StorageCodeMinLengthMessage.Instance)
			.EnsureMaxLength(128, StorageCodeMaxLengthMessage.Instance);

	public void SetType(StorageType type)
	{
		Type = type;
	}

	public void SetDescription(string? description)
		=> Description = description
			.NullIfWhiteSpace()
			?.EnsureMaxLength(256, StorageDescriptionMaxLengthMessage.Instance);

	public void SetLocation(string? location)
		=> Location =
			location.NullIfWhiteSpace()?.EnsureMaxLength(256, StorageLocationMaxLengthMessage.Instance);

	public override string GetId() => Code;
}
