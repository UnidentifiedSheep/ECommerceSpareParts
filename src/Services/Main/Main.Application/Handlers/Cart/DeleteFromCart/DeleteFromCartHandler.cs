using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Cart;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Cart.DeleteFromCart;

[Transactional]
public record DeleteFromCartCommand(Guid UserId, int ArticleId) : ICommand;

public class DeleteFromCartHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork) : ICommandHandler<DeleteFromCartCommand>
{
    public async Task<Unit> Handle(DeleteFromCartCommand request, CancellationToken cancellationToken)
    {
        var cartItem =
            await cartRepository.GetCartItemAsync(request.UserId, request.ArticleId, true, cancellationToken) ??
            throw new CartItemNotFoundException(request.ArticleId);
        
        unitOfWork.Remove(cartItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}