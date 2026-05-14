using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Extensions;

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

    public static UserDiscount Create(Guid userId, decimal discount)
    {
        return new UserDiscount(userId, discount);
    }

    internal void SetDiscount(decimal discount)
    {
        Discount = discount.AgainstOutOfRange(0m, 0.99m, "user.discount.range");
    }

    public override Guid GetId()
    {
        return UserId;
    }

    public static Expression<Func<UserDiscount, bool>> GetEqualityExpression(Guid key)
        => x => x.UserId == key;
}