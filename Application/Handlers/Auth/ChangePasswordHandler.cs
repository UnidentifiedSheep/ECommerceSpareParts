using Application.Interfaces;
using MediatR;

namespace Application.Handlers.Auth;

public record ChangePasswordCommand : ICommand;

public class ChangePasswordHandler() : ICommandHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}