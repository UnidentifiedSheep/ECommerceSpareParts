using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Application.Common.Interfaces;
using Attributes;
using Extensions;
using Main.Abstractions.Dtos.Emails;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.Users.CreateUser;

[Transactional]
public record CreateUserCommand(string UserName, string Password, UserInfoDto UserInfo, IEnumerable<EmailDto> Emails,
    IEnumerable<string> Phones, IEnumerable<string> Roles) : ICommand<CreateUserResult>;

public record CreateUserResult(Guid UserId);

public class CreateUserHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork, IPasswordManager passwordManager) 
    : ICommandHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        
        var roles = await roleRepository.GetRolesAsync(request.Roles, true, cancellationToken);
        var passwordHash = passwordManager.GetHashOfPassword(request.Password);
        var user = new User
        {
            UserName = userName,
            NormalizedUserName = userName.ToNormalized(),
            PasswordHash = passwordHash,
            UserRoles = roles.Select(x => new UserRole
            {
                RoleId = x.Id
            }).ToList(),
            UserEmails = request.Emails.Adapt<List<UserEmail>>(),
            UserInfo = request.UserInfo.Adapt<UserInfo>()
        };

        await unitOfWork.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateUserResult(user.Id);
    }
}