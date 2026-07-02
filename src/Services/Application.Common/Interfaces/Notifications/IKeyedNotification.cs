using MediatR;

namespace Application.Common.Interfaces.Notifications;

public interface IKeyedNotification : INotification
{
    string GetKey();
}