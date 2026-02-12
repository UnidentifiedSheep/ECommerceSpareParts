using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Enums;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.Handlers.ArticleWeight.SetArticleWeight;

[Transactional]
public record SetArticleWeightCommand(int ArticleId, decimal Weight, WeightUnit Unit) : ICommand;

public class SetArticleWeightHandler(IArticleWeightRepository weightRepository, IMediator mediator,
    IUnitOfWork unitOfWork) : ICommandHandler<SetArticleWeightCommand>
{
    public  async Task<Unit> Handle(SetArticleWeightCommand request, CancellationToken cancellationToken)
    {
        var weight = await weightRepository
            .GetArticleWeight(request.ArticleId, true, cancellationToken);
        
        if (weight == null)
        {
            weight = new Entities.ArticleWeight { ArticleId = request.ArticleId };
            await unitOfWork.AddAsync(weight, cancellationToken);
        }
        
        weight.Weight = request.Weight;
        weight.Unit = request.Unit;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        //publish notification to invalidate cache
        await mediator.Publish(new ArticleWeightUpdatedNotification(request.ArticleId), cancellationToken);
        return Unit.Value;
    }
}