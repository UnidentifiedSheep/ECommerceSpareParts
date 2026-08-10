using Abstractions.Models.Options;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Application.Dtos.Emails;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Main.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Users.AddEmailToUser;

[Diagnostics(maxExecutionTimeMs: 300)]
[Transactional]
[AutoSave]
public record AddEmailToUserCommand(
    Guid UserId,
    string Email,
    EmailType EmailType) : ICommand<AddEmailToUserResult>;

public record AddEmailToUserResult(
    Guid UserId,
    string Email);

public class AddEmailToUserHandler(
    IOptions<UserEmailOptions> options,
    IUserRepository userRepository,
    IReadRepository<UserEmail, string> emailRepository)
    : ICommandHandler<AddEmailToUserCommand, AddEmailToUserResult>
{
    public async Task<AddEmailToUserResult> Handle(
        AddEmailToUserCommand request,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<User>.New()
            .Where(x => x.Id == request.UserId)
            .WhereDoesNotHaveRole(Role.System)
            .Include(x => x.Emails)
            .Track()
            .Build();

        var user = await userRepository.FirstOrDefaultAsync(
                       criteria,
                       cancellationToken)
                   ?? throw new UserNotFoundException(request.UserId);

        Email email = request.Email;

        var emailAlreadyInUse = await emailRepository.Query
            .AsNoTracking()
            .AnyAsync(
                x => x.Email == email && x.UserId != user.Id,
                cancellationToken);

        if (emailAlreadyInUse)
            throw new UserEmailAlreadyInUseException(request.Email);

        user.AddEmail(
            email,
            request.EmailType,
            options.Value.MaxEmailCount);

        return new AddEmailToUserResult(user.Id, email.Value);
    }
}
