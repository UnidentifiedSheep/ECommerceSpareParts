using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Tests.Persistence.Entities;

internal sealed class CompositeKeyTestEntity : Entity<CompositeKeyTestEntity, CompositeKeyTestKey>,
	ILinqEntity<CompositeKeyTestEntity, CompositeKeyTestKey>
{
	private CompositeKeyTestEntity() { }

	public CompositeKeyTestEntity(string code, int number)
	{
		Code = code;
		Number = number;
	}

	public string Code { get; private set; } = null!;

	public int Number { get; private set; }

	public static Expression<Func<CompositeKeyTestEntity, CompositeKeyTestKey>> GetKeySelector()
		=> x => new CompositeKeyTestKey(x.Code, x.Number);

	public static Expression<Func<CompositeKeyTestEntity, bool>> GetEqualityExpression(CompositeKeyTestKey key)
		=> x => x.Code == key.Code && x.Number == key.Number;

	public override CompositeKeyTestKey GetId() => new(Code, Number);
}

internal readonly record struct CompositeKeyTestKey(string Code, int Number) : ICompositeKey
{
	public object[] ToArray() => [Code, Number];
}
