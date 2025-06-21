using Core.Interface;
using Microsoft.AspNetCore.Identity;
using MonoliteUnicorn.PostGres.Identity;

namespace MonoliteUnicorn.EndPoints.Auth.ChangePassword;

public record ChangePasswordCommand() : ICommand<ChangePasswordResult>;
public record ChangePasswordResult(bool Succeed);

public class ChangePasswordHandler(UserManager<UserModel> userManager) : ICommandHandler<ChangePasswordCommand, ChangePasswordResult>
{
    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}