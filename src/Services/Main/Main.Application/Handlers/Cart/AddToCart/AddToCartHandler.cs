using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using MediatR;

namespace Main.Application.Handlers.Cart.AddToCart;

[Transactional]
public record AddToCartCommand(Guid UserId, int ArticleId, int Count) : ICommand;

public class AddToCartHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddToCartCommand>
{
    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cartItem = new Entities.Cart
        {
            UserId = request.UserId,
            ArticleId = request.ArticleId,
            Count = request.Count
        };
        await unitOfWork.AddAsync(cartItem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}