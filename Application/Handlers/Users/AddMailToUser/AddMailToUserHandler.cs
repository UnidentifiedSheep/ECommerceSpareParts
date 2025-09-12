using Application.Interfaces;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Core.Models;
using Exceptions.Exceptions.Users;
using Mapster;
using MediatR;

namespace Application.Handlers.Users.AddMailToUser;

public record AddMailToUserCommand(string UserId, string Email, bool IsVerified) : ICommand<Unit>;

public class AddMailToUserHandler(
    IUserEmailRepository emailRepository,
    IEmailValidator emailValidator,
    IUnitOfWork unitOfWork) : ICommandHandler<AddMailToUserCommand, Unit>
{
    public async Task<Unit> Handle(AddMailToUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var verified = request.IsVerified;
        await ValidateData(email, cancellationToken);

        var model = Email.Create(email, emailValidator).Adapt<UserMail>();
        model.IsVerified = verified;
        model.UserId = request.UserId;

        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(string email, CancellationToken ct = default)
    {
        if (await emailRepository.EmailTaken(email, ct))
            throw new EmailAlreadyTakenException(email);
    }
}