using Core.Extensions;
using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Users;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Users.CreateUser;

public record CreateUserCommand(AmwNewUserDto NewUser) : ICommand<CreateUserResult>;
public record CreateUserResult(string UserId);

public class CreateUserValidation : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidation()
    {
        //Name Rules
        RuleFor(x => x.NewUser).Must(x => !string.IsNullOrWhiteSpace(x.Name)).WithMessage("Имя пользователя не должно быть пустым");
        RuleFor(x => x.NewUser).Must(x => x.Name.Trim().Length >= 3)
            .WithMessage("Имя пользователя должно состоять минимум из 3 символов");
        //User Name Rules
        RuleFor(x => x.NewUser).Must(x => !string.IsNullOrWhiteSpace(x.UserName))
            .WithMessage("Логин пользователя не может быть пустым");
        RuleFor(x => x.NewUser).Must(x => x.UserName.Trim().Length >= 3)
            .WithMessage("Логин пользователя должен состоять минимум из 3 символов");
        //Email Rules 
        RuleFor(x => x.NewUser).Must(x => string.IsNullOrWhiteSpace(x.Email) || x.Email.IsValidMail())
            .WithMessage(x => $"'{x.NewUser.Email}' не является валидной почтой");
        //Phone Number
        RuleFor(x => x.NewUser).Must(x => string.IsNullOrWhiteSpace(x.PhoneNumber) || x.PhoneNumber.IsValidPhoneNumber())
            .WithMessage(x => $"'{x.NewUser.PhoneNumber}' не является валидным номером телефона");
    }
}

public class CreateUserHandler(DContext context) : ICommandHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        request.NewUser.Email = request.NewUser.Email?.Trim();
        var userNameTaken = await context.AspNetUsers.AsNoTracking()
            .AnyAsync(x => x.NormalizedUserName == request.NewUser.UserName.ToUpperInvariant(), cancellationToken);
        var sameMail = await context.UserMails.AsNoTracking()
            .AnyAsync(x => !string.IsNullOrWhiteSpace(request.NewUser.Email) &&
                           x.NormalizedEmail == request.NewUser.Email.ToUpperInvariant(), cancellationToken: cancellationToken);
        if (userNameTaken) throw new UserNameAlreadyTakenException(request.NewUser.UserName);
        if (sameMail) throw new EmailAlreadyTakenException(request.NewUser.Email);
        request.NewUser.Roles = new HashSet<string>(request.NewUser.Roles.Select(x => x.ToUpperInvariant()));
        var roles = await context.AspNetRoles
            .Where(x => request.NewUser.Roles.Count > 0 &&
                        x.NormalizedName != null &&
                        request.NewUser.Roles.Contains(x.NormalizedName))
            .ToDictionaryAsync(x => x.Id, cancellationToken: cancellationToken);
        if (roles.Count < request.NewUser.Roles.Count && request.NewUser.Roles.Count > 0)
        {
            var diff = request.NewUser.Roles
                .Where(x => !roles.Values.Select(z => z.NormalizedName).Contains(x));
            throw new RoleNotFoundException(diff);
        }
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var user = request.NewUser.Adapt<AspNetUser>();
        if (roles.Count > 0) user.Roles = roles.Values;
        if (!string.IsNullOrWhiteSpace(request.NewUser.Email))
            user.UserMails.Add(new UserMail
            {
                Email = request.NewUser.Email,
                IsVerified = false,
                NormalizedEmail = request.NewUser.Email.ToUpperInvariant()
            });
        await context.AspNetUsers.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new CreateUserResult(user.Id);
    }
}