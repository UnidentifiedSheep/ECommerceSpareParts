using Application.Common.Interfaces;
using Core.Attributes;
using MediatR;

namespace Main.Application.Handlers.Auth.ConfirmMail;

[Transactional]
public record ConfirmMailCommand(string UserId, string ConfirmationToken) : ICommand;

public class ConfirmMailHandler : ICommandHandler<ConfirmMailCommand>
{
    public async Task<Unit> Handle(ConfirmMailCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        /*var user = await manager.FindByIdAsync(request.UserId);
        if (user == null) throw new UserNotFoundException();
        var confirmed = await manager.ConfirmEmailAsync(user, request.ConfirmationToken);
        if (!confirmed.Succeeded) throw new InvalidTokenException($"{request.ConfirmationToken} не является валидным.");
        return Unit.Value;*/
    }
}