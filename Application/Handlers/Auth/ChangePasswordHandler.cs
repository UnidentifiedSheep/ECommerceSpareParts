using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Entities;

namespace Application.Handlers.Auth;

public record ChangePasswordCommand() : ICommand;
public class ChangePasswordHandler(UserManager<UserModel> userManager) : ICommandHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}