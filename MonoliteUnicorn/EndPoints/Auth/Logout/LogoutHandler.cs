using Core.Interface;
using Core.Redis;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth.Logout;

public record LogoutCommand(string Token, string UserId) : ICommand;
public class LogoutHandler : ICommandHandler<LogoutCommand>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var redis = Redis.GetRedis();
        await redis.KeyDeleteAsync(request.Token);
        await redis.SetRemoveAsync(request.UserId, request.Token);
        return Unit.Value;
    }
}