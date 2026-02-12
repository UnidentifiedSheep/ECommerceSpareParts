using Contracts.ArticleCoefficients.GetArticleCoefficients;
using Contracts.Models.ArticleCoefficients;
using Main.Application.Handlers.ArticleCoefficient.GetArticleCoefficients;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Consumers;

public class GetArticleCoefficientsConsumer(IMediator mediator) : IConsumer<GetArticleCoefficientsRequest>
{
    public async Task Consume(ConsumeContext<GetArticleCoefficientsRequest> context)
    {
        var result = await mediator.Send(new GetArticleCoefficientsQuery(context.Message.ArticleIds));

        var adapted = result.Coefficients.ToDictionary(x => x.Key,
            x => x.Value.Adapt<List<ArticleCoefficient>>());

        await context.RespondAsync<GetArticleCoefficientsResponse>(adapted);
    }
}