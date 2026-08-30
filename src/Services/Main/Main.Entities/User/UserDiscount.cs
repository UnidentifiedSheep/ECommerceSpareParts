using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.DomainEvents.User;

namespace Main.Entities.User;

public class UserDiscount : Entity<UserDiscount, Guid>, ILinqEntity<UserDiscount, Guid>
{
	private UserDiscount()
	{
	}

	private UserDiscount(Guid userId, decimal discount)
	{
		UserId = userId;
		SetDiscount(discount);
	}

	public Guid UserId { get; set; }

	public decimal Discount { get; set; }

	public static Expression<Func<UserDiscount, Guid>> GetKeySelector() => x => x.UserId;

	public static Expression<Func<UserDiscount, bool>> GetEqualityExpression(Guid key) => x =>
		x.UserId == key;

	public static UserDiscount Create(Guid userId, decimal discount) => new(userId, discount);

	internal void SetDiscount(decimal discount)
	{
		Discount = discount.EnsureInRange(
			0m,
			0.99m,
			"user.discount.range");
	}

	public override void OnCreated() => AddDomainEvent(new UserUpdatedDomainEvent(UserId));

	public override void OnUpdated() => AddDomainEvent(new UserUpdatedDomainEvent(UserId));

	public override void OnDeleted() => AddDomainEvent(new UserUpdatedDomainEvent(UserId));

	public override Guid GetId() => UserId;
}
