using Abstractions.Models.Options;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.User;
using Enums;
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
    EmailType EmailType) : ICommand;

public class AddEmailToUserHandler(
    IOptions<UserEmailOptions> options,
    IUserRepository userRepository,
    IReadRepository<UserEmail, string> emailRepository,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<AddEmailToUserCommand>
{
    public async Task<Unit> Handle(
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

        integrationEventScope.Add(
            new UserUpdatedEvent
            {
                UserId = request.UserId
            });

        return Unit.Value;
    }
}
