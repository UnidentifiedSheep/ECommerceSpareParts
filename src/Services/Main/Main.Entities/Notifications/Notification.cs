using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Notifications;

public class Notification : Entity<Notification, int>, ILinqEntity<Notification, int>
{
	public int Id { get; private set; }
	public Guid UserId { get; private set; }

	public string Title { get; private set; } = null!;
	public string Message { get; private set; } = null!;

	public DateTime PublishedAt { get; private set; }
	public DateTime? SeenAt { get; private set; }

	public User.User User { get; private set; } = null!;

	public override int GetId() => Id;
	public static Expression<Func<Notification, int>> GetKeySelector() => x => x.Id;
	public static Expression<Func<Notification, bool>> GetEqualityExpression(int key) => x => x.Id == key;
}
