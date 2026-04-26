using Contracts.ArticleCoefficients.GetArticleCoefficients;
using Contracts.Models.ArticleCoefficients;
using Contracts.Models.Coefficients;
using Main.Application.Handlers.ProductCoefficient.GetArticleCoefficients;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Consumers;

public class GetArticleCoefficientsConsumer(IMediator mediator) : IConsumer<GetArticleCoefficientsRequest>
{
    public async Task Consume(ConsumeContext<GetArticleCoefficientsRequest> context)
    {
        var result = await mediator.Send(new GetProductCoefficientsQuery(context.Message.ArticleIds));

        var response = new GetArticleCoefficientsResponse
        {
            Coefficients = result.Coefficients
                .GroupBy(x => x.ProductId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(z => new ArticleCoefficient
                    {
                        ArticleId = z.ProductId,
                        Coefficient = new Coefficient
                        {
                            Name = z.Coefficient.Name,
                            Type = z.Coefficient.Type,
                            Value = z.Coefficient.Value
                        },
                        CreatedAt = z.CreatedAt,
                        ValidTill = z.ValidTill
                    }).ToList())
        };

        await context.RespondAsync(response);
    }
}