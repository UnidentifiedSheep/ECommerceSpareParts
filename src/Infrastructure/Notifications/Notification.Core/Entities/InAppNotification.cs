using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;

namespace Notification.Core.Entities;

public sealed class InAppNotification : Entity<InAppNotification, int>, ILinqEntity<InAppNotification, int>
{
	public int Id { get; private set; }
	public Guid UserId { get; private set; }
	public string Text { get; private set; } = null!;
	public DateTime CreateAt { get; private set; }
	public DateTime? SeenAt { get; private set; }
	public bool IsSeen => SeenAt.HasValue;

	private InAppNotification() { }

	private InAppNotification(Guid userId, string text)
	{
		UserId = userId;
		Text = text.EnsureNotNullOrWhiteSpace(
			() => new InvalidOperationException("In-app notification text must not be null or empty."));
		CreateAt = DateTime.UtcNow;
	}

	public static InAppNotification Create(Guid userId, string text) => new(userId, text);

	public void See() => SeenAt ??= DateTime.UtcNow;

	public void UnSee() => SeenAt = null;

	public override int GetId() => Id;

	public static Expression<Func<InAppNotification, int>> GetKeySelector() => x => x.Id;

	public static Expression<Func<InAppNotification, bool>> GetEqualityExpression(int key) =>
		x => x.Id == key;
}
