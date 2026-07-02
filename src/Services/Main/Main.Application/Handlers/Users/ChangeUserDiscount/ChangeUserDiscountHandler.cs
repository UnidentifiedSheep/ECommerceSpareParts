using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.User;
using Main.Entities.Exceptions;
using Main.Entities.User;
using MediatR;

namespace Main.Application.Handlers.Users.ChangeUserDiscount;

[AutoSave]
[Transactional]
public record ChangeUserDiscountCommand(Guid UserId, decimal Discount) : ICommand;

public class ChangeUserDiscountHandler(
    IRepository<User, Guid> usersRepository,
    IIntegrationEventScope integrationEventScope
)
    : ICommandHandler<ChangeUserDiscountCommand>
{
    public async Task<Unit> Handle(ChangeUserDiscountCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var criteria = Criteria<User>.New()
            .Track()
            .Where(x => x.Id == userId)
            .Include(x => x.Discount)
            .Build();

        var user = await usersRepository.FirstOrDefaultAsync(criteria, cancellationToken)
                   ?? throw new UserNotFoundException(userId);

        user.SetDiscount(request.Discount);

        integrationEventScope.Add(
            new UserDiscountUpdatedEvent
            {
                UserId = userId,
                Discount = request.Discount,
                ChangedAt = DateTime.UtcNow
            });

        return Unit.Value;
    }
}