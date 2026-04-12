using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Enums;
using Main.Application.Interfaces.Repositories;
using Main.Application.Notifications;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ArticleWeight.SetArticleWeight;

[AutoSave]
[Transactional]
public record SetArticleWeightCommand(int ProductId, decimal Weight, WeightUnit Unit) : ICommand;

public class SetProductWeightHandler(
    IPublisher publisher,
    IProductWeightRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetArticleWeightCommand>
{
    public async Task<Unit> Handle(SetArticleWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await repository.GetById(request.ProductId, cancellationToken);

        if (weight == null)
        {
            weight = ProductWeight.Create(request.ProductId, request.Weight, request.Unit);
            await unitOfWork.AddAsync(weight, cancellationToken);
        }
        else
            weight.Update(request.Weight, request.Unit);
        
        await publisher.Publish(new ArticleWeightUpdatedNotification(request.ProductId), cancellationToken);
        return Unit.Value;
    }
}