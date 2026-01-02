using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Users;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.ArticleReservations;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleReservations.CreateArticleReservation;

[Transactional]
public record CreateArticleReservationCommand(List<NewArticleReservationDto> Reservations, Guid WhoCreated) : ICommand;

public class CreateArticleReservationHandler(
    DbDataValidatorBase dbValidator,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateArticleReservationCommand>
{
    public async Task<Unit> Handle(CreateArticleReservationCommand request, CancellationToken cancellationToken)
    {
        var reservations = request.Reservations.ToList();
        var currencyIds = new HashSet<int>();
        var articleIds = new HashSet<int>();
        var userIds = new HashSet<Guid> { request.WhoCreated };

        foreach (var item in reservations)
        {
            if (item.GivenCurrencyId != null)
                currencyIds.Add(item.GivenCurrencyId.Value);
            articleIds.Add(item.ArticleId);
            userIds.Add(item.UserId);
        }

        await CheckIfNeededExists(currencyIds, articleIds, userIds, cancellationToken);

        var adaptedReservations = reservations.Adapt<List<StorageContentReservation>>();
        foreach (var item in adaptedReservations)
            item.WhoCreated = request.WhoCreated;
        await unitOfWork.AddRangeAsync(adaptedReservations, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task CheckIfNeededExists(IEnumerable<int> currencyIds, IEnumerable<int> articleIds,
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan()
            .EnsureCurrencyExists(currencyIds)
            .EnsureUserExists(userIds)
            .EnsureArticleExists(articleIds);
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}