using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Services.Articles;
using Main.Core.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Articles.CreateArticles;

[Transactional]
public record CreateArticlesCommand(List<CreateArticleDto> NewArticles) : ICommand;

public class CreateArticlesHandler(IUnitOfWork unitOfWork, DbDataValidatorBase dbValidator)
    : ICommandHandler<CreateArticlesCommand>
{
    public async Task<Unit> Handle(CreateArticlesCommand request, CancellationToken cancellationToken)
    {
        var producersIds = request.NewArticles.Select(x => x.ProducerId);

        var plan = new ValidationPlan().EnsureProducerExists(producersIds);
        await dbValidator.Validate(plan, true, true, cancellationToken);

        var articles = request.NewArticles.Adapt<List<Article>>();
        await unitOfWork.AddRangeAsync(articles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}