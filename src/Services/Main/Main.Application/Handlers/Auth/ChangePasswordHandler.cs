using Application.Common.Interfaces;
using MediatR;

namespace Main.Application.Handlers.Auth;

public record ChangePasswordCommand : ICommand;

public class ChangePasswordHandler : ICommandHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}