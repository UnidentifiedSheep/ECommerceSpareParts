using Main.Application.Extensions;
using Core.Attributes;
using Core.Dtos.Services.Articles;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Main.Application.Interfaces;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Articles.CreateArticles;

[Transactional]
public record CreateArticlesCommand(List<CreateArticleDto> NewArticles) : ICommand;

public class CreateArticlesHandler(IProducerRepository producerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateArticlesCommand>
{
    public async Task<Unit> Handle(CreateArticlesCommand request, CancellationToken cancellationToken)
    {
        var producersIds = request.NewArticles.Select(x => x.ProducerId);

        await producerRepository.EnsureProducersExists(producersIds, cancellationToken);

        var articles = request.NewArticles.Adapt<List<Article>>();
        await unitOfWork.AddRangeAsync(articles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}