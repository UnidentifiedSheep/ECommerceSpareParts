using Abstractions.Models.Options;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.User;
using Enums;
using Exceptions;
using Main.Application.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using Main.Entities.User;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Users.RemoveEmailFromUser;

[Diagnostics(maxExecutionTimeMs: 300)]
[Transactional]
[AutoSave]
public record RemoveEmailFromUserCommand(Guid UserId, string Email) : ICommand;

public class RemoveEmailFromUserHandler(
    IOptions<UserEmailOptions> options,
    IIntegrationEventScope integrationEventScope,
    IUserRepository repository
) : ICommandHandler<RemoveEmailFromUserCommand>
{
    public async Task<Unit> Handle(
        RemoveEmailFromUserCommand request,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<User>.New()
            .Where(x => x.Id == request.UserId)
            .WhereDoesNotHaveRole(Role.System)
            .Include(x => x.Emails)
            .Track()
            .Build();

        var user = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
                   ?? throw new UserNotFoundException(request.UserId);

        if (options.Value.MinEmailCount > user.Emails.Count - 1)
            throw new InvalidInputException("user.min.email.count", [options.Value.MinEmailCount]);

        user.RemoveUserEmail(request.Email);

        integrationEventScope.Add(
            new UserUpdatedEvent
            {
                UserId = request.UserId
            });

        return Unit.Value;
    }
}