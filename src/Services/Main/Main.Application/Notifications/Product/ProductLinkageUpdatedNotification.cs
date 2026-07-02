using MediatR;

namespace Main.Application.Notifications;

public record ProductLinkageUpdatedNotification : INotification
{
    public IReadOnlyList<int> ProductIds { get; }
    
    public ProductLinkageUpdatedNotification(IEnumerable<int> ids)
    {
        ProductIds = ids.Distinct().ToList();
    }
    
    public ProductLinkageUpdatedNotification(int id) : this([id]) {}
}