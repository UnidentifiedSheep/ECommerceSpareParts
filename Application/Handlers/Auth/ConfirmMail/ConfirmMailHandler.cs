using Application.Extensions;
using Application.Interfaces;
using Core.Attributes;
using Core.Exceptions.Auth;
using Core.Exceptions.Users;
using Core.Interfaces.DbRepositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Entities;

namespace Application.Handlers.Auth.ConfirmMail;

[Transactional]
public record ConfirmMailCommand(string UserId, string ConfirmationToken) : ICommand;
public class ConfirmMailHandler(UserManager<UserModel> manager) : ICommandHandler<ConfirmMailCommand>
{
    public async Task<Unit> Handle(ConfirmMailCommand request, CancellationToken cancellationToken)
    {
        var user = await manager.FindByIdAsync(request.UserId);
        if (user == null) throw new UserNotFoundException();
        var confirmed = await manager.ConfirmEmailAsync(user, request.ConfirmationToken);
        if (!confirmed.Succeeded) throw new InvalidTokenException($"{request.ConfirmationToken} не является валидным.");
        return Unit.Value;
    }
}