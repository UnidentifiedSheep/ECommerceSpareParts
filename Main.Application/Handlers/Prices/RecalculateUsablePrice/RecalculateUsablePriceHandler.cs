using Core.Interfaces.Services;
using Main.Application.Interfaces;
using MediatR;

namespace Main.Application.Handlers.Prices.RecalculateUsablePrice;

public record RecalculateUsablePriceCommand(IEnumerable<int> ArticleIds) : ICommand;

public class RecalculateUsablePriceHandler(IArticlePricesService pricesService)
    : ICommandHandler<RecalculateUsablePriceCommand>
{
    public async Task<Unit> Handle(RecalculateUsablePriceCommand request, CancellationToken cancellationToken)
    {
        await pricesService.RecalculateUsablePrice(request.ArticleIds, cancellationToken);
        return Unit.Value;
    }
}