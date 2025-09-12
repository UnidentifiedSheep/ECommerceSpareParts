using Application.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.Users;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Users;
using Mapster;

namespace Application.Handlers.Users.CreateUser;

[Transactional]
public record CreateUserCommand(NewUserDto NewUser) : ICommand<CreateUserResult>;

public record CreateUserResult(string UserId);

public class CreateUserHandler(
    IUsersRepository usersRepository,
    IUserEmailRepository emailRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var newUser = request.NewUser;
        await ValidateData(newUser.UserName, newUser.Email, newUser.Roles, cancellationToken);

        var model = newUser.Adapt<AspNetUser>();
        if (model.Email != null)
        {
            var emailModel = new UserMail
            {
                Email = model.Email,
                NormalizedEmail = model.NormalizedEmail!,
                IsVerified = model.EmailConfirmed,
                LocalPart = model.Email.Split('@').First(),
                UserId = model.Id
            };
            await unitOfWork.AddAsync(emailModel, cancellationToken);
        }

        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateUserResult(model.Id);
    }

    private async Task ValidateData(string username, string? email, IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        if (await usersRepository.UserNameTaken(username, ct))
            throw new UserNameAlreadyTakenException(username);
        if (!string.IsNullOrWhiteSpace(email) && await emailRepository.EmailTaken(email, ct))
            throw new EmailAlreadyTakenException(email);
    }
}