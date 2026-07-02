using MediatR;

namespace Main.Application.Notifications;

public record ProductSizeUpdatedNotification : INotification
{
    public IReadOnlyList<int> ProductIds { get; }
    public ProductSizeUpdatedNotification(IEnumerable<int> ids)
    {
        ProductIds = ids.Distinct().ToList();
    }
    public ProductSizeUpdatedNotification(int id) : this([id]) {}
}