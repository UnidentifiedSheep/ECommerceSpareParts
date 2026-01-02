using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Core.Interfaces.Validators;
using Exceptions.Exceptions.Roles;
using Exceptions.Exceptions.Users;
using Main.Application.Extensions;
using Main.Core.Dtos.Emails;
using Main.Core.Dtos.Users;
using Main.Core.Entities;
using Main.Core.Extensions;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Users.CreateUser;

[Transactional]
public record CreateUserCommand(
    string UserName,
    string Password,
    UserInfoDto UserInfo,
    IEnumerable<EmailDto> Emails,
    IEnumerable<string> Phones,
    IEnumerable<string> Roles) : ICommand<CreateUserResult>;

public record CreateUserResult(Guid UserId);

public class CreateUserHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserEmailRepository userEmailRepository,
    IUnitOfWork unitOfWork,
    IPasswordManager passwordManager) : ICommandHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        var emails = request.Emails.Select(x => x.Email.Trim()).ToHashSet();
        //Add Phone Number addition logic to it.
        await ValidateData(userName, emails, request.Phones, request.Roles, cancellationToken);
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

    private async Task ValidateData(string userName, HashSet<string> emails, IEnumerable<string> phones,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        await roleRepository.EnsureRolesExists(roles, cancellationToken);
        if (await userRepository.IsUserNameTakenAsync(userName, cancellationToken))
            throw new UserNameAlreadyTakenException(userName);
        if (emails.Count > 0)
        {
            var notFound = (await userEmailRepository.IsEmailsExists(emails, cancellationToken))
                .ToList();
            if (notFound.Count != emails.Count)
                throw new EmailAlreadyTakenException(emails.Except(notFound));
        }
    }
}