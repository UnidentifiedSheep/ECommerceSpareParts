using Core.Extensions;
using Core.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Users.AddMailToUser;

public record AddMailToUserCommand(string UserId, string Email, string? Comment, bool IsVerified) : ICommand<Unit>;

public class AddMailToUserValidation : AbstractValidator<AddMailToUserCommand>
{
    public AddMailToUserValidation()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Id пользователя не может быть пустым");
        RuleFor(x => x.Email).Must(x => x.IsValidMail()).WithMessage("Указанная почта не является валидной.");
    }
}

public class AddMailToUserHandler(DContext context) : ICommandHandler<AddMailToUserCommand, Unit>
{
    public async Task<Unit> Handle(AddMailToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.AspNetUsers.AsNoTracking().AnyAsync(x => x.Id == request.UserId, cancellationToken);
        if (!user) throw new UserNotFoundException(request.UserId);
        var normalizedEmail = request.Email.ToUpperInvariant();
        if (await context.UserMails.AsNoTracking().AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken: cancellationToken))
            throw new EmailAlreadyTakenException(request.Email);
        var userMail = new UserMail
        {
            Email = request.Email,
            UserId = request.UserId,
            IsVerified = request.IsVerified,
            NormalizedEmail = normalizedEmail,
        };
        await context.UserMails.AddAsync(userMail, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}