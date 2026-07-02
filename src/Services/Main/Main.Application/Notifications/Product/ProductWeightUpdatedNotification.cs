using MediatR;

namespace Main.Application.Notifications;

public record ProductWeightUpdatedNotification : INotification
{
    public IReadOnlyList<int> ProductIds { get; }
    public ProductWeightUpdatedNotification(IEnumerable<int> ids)
    {
        ProductIds = ids.Distinct().ToList();
    }
    public ProductWeightUpdatedNotification(int id) : this([id]) {}
}