using Core.Interface;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MonoliteUnicorn.PostGres.Identity;

namespace MonoliteUnicorn.EndPoints.Auth.ChangePassword;

public record ChangePasswordCommand() : ICommand;
public class ChangePasswordHandler(UserManager<UserModel> userManager) : ICommandHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}