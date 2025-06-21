using Core.Extensions;
using Core.Interface;
using Core.Services.TimeWebCloud.Interfaces;
using Core.StaticFunctions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Users.CreateMailForUser;

public record CreateMailForUserCommand(string UserId, string MailBox, string? Password, string? Comment) : ICommand<CreateMailForUserResult>;
public record CreateMailForUserResult(string MailBoxAddress, string Password);

public class CreateMailForUserValidation : AbstractValidator<CreateMailForUserCommand>
{
    public CreateMailForUserValidation()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Id пользователя не может быть пустым");
        RuleFor(x => x.MailBox).NotEmpty().WithMessage("Адрес почты не может быть пустым");
        RuleFor(x => x.Password)
            .Must(x => string.IsNullOrWhiteSpace(x) || (x.Length >= 8 && x == x.Trim()))
            .WithMessage("Пароль должен быть не короче 8 символов и не содержать пробелов в начале или конце");
        RuleFor(x => x.MailBox).Must(x => $"{x.Trim()}@{Global.Domain}".IsValidMail())
            .WithMessage("Указанная почта не является валидной.");
    }
}

public class CreateMailForUserHandler(DContext context, ITimeWebMail timeWebMail) : ICommandHandler<CreateMailForUserCommand, CreateMailForUserResult>
{
    public async Task<CreateMailForUserResult> Handle(CreateMailForUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.AspNetUsers.AsNoTracking().AnyAsync(x => x.Id == request.UserId, cancellationToken);
        if (!user) throw new UserNotFoundException(request.UserId);
        
        var password = string.IsNullOrWhiteSpace(request.Password) ? Generator.GeneratePassword(12) : request.Password.Trim();
        var mailBox = request.MailBox.Trim();
        var email = $"{mailBox}@{Global.Domain}";
        
        await timeWebMail.CreateMail(Global.Domain, mailBox, password, request.Comment ?? "", cancellationToken);
        var userMail = new UserMail
        {
            Email = email,
            UserId = request.UserId,
            IsVerified = true,
            NormalizedEmail = email.ToUpperInvariant()
        };
        await context.UserMails.AddAsync(userMail, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return new CreateMailForUserResult($"{mailBox}@{Global.Domain}", password);
    }
}