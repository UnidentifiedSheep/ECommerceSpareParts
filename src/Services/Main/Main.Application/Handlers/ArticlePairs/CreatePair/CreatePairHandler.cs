using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticlePairs.CreatePair;

[Transactional]
public record CreatePairCommand(int LeftArticleId, int RightArticleId) : ICommand;

public class CreatePairHandler(IUnitOfWork unitOfWork, DbDataValidatorBase dbValidator)
    : ICommandHandler<CreatePairCommand>
{
    public async Task<Unit> Handle(CreatePairCommand request, CancellationToken cancellationToken)
    {
        //Ensuring that articles exist
        var plan = new ValidationPlan().EnsureArticleExists([request.LeftArticleId, request.RightArticleId]);
        await dbValidator.Validate(plan, true, true, cancellationToken);
        
        var leftPair = request.Adapt<ArticlesPair>();
        var rightPair = request.Adapt<ArticlesPair>();

        // переопределяем только для обратной пары
        rightPair.ArticleLeft = request.RightArticleId;
        rightPair.ArticleRight = request.LeftArticleId;

        await unitOfWork.AddRangeAsync([leftPair, rightPair], cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}