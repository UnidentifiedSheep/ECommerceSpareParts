using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Users;
using Main.Application.Projections;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Role = Main.Enums.Role;

namespace Main.Application.Handlers.Users.CreateUser;

[Transactional]
public record CreateUserCommand(
    string UserName,
    string Password,
    UserInfoDto UserInfo,
    IEnumerable<EmailDto> Emails,
    IEnumerable<UserPhoneDto> Phones,
    IEnumerable<string> Roles) : ICommand<CreateUserResult>;

public record CreateUserResult(UserDto User);

public class CreateUserHandler(IUnitOfWork unitOfWork, IPasswordManager passwordManager)
    : ICommandHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var passwordHash = passwordManager.GetHashOfPassword(request.Password);
        var user = User.Create(request.UserName, passwordHash);
        user.SetUserInfo(request.UserInfo.Name, request.UserInfo.Surname, request.UserInfo.Description);

        foreach (var role in request.Roles)
        {
            if (role == RoleNames.Normalize(nameof(Role.System)))
                throw new CantCreateSystemUserException();
            user.AddRole(role);
        }

        foreach (var email in request.Emails)
            user.AddUserEmail(email.Email, email.Type, email.IsPrimary, email.IsConfirmed);

        foreach (var phone in request.Phones)
            user.AddUserPhone(phone.Number, phone.Type, phone.IsPrimary, phone.IsConfirmed);

        await unitOfWork.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateUserResult(UserProjections.UserProjection.AsFunc()(user));
    }
}