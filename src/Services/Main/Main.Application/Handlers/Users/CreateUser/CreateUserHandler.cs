using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Application.Common.Interfaces;
using Attributes;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Users;
using Main.Entities.User;

namespace Main.Application.Handlers.Users.CreateUser;

[Transactional]
public record CreateUserCommand(
    string UserName,
    string Password,
    UserInfoDto UserInfo,
    IEnumerable<EmailDto> Emails,
    IEnumerable<string> Roles) : ICommand<CreateUserResult>;

public record CreateUserResult(Guid UserId);

public class CreateUserHandler(IUnitOfWork unitOfWork, IPasswordManager passwordManager)
    : ICommandHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var passwordHash = passwordManager.GetHashOfPassword(request.Password);
        var user = User.Create(request.UserName, passwordHash);
        user.SetUserInfo(request.UserInfo.Name, request.UserInfo.Surname, request.UserInfo.Description);

        foreach (var role in request.Roles)
            user.AddUserRole(role);

        foreach (var email in request.Emails)
            user.AddUserEmail(email.Email, email.Type, email.IsPrimary, email.IsConfirmed);

        await unitOfWork.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateUserResult(user.Id);
    }
}