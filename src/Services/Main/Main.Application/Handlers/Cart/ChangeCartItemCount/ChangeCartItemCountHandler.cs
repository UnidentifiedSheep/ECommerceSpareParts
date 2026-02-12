using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.Cart;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Cart.ChangeCartItemCount;

[Transactional]
public record ChangeCartItemCountCommand(Guid UserId, int ArticleId, int NewCount) : ICommand;

public class ChangeCartItemCountHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork) : ICommandHandler<ChangeCartItemCountCommand>
{
    public async Task<Unit> Handle(ChangeCartItemCountCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await cartRepository.GetCartItemAsync(request.UserId, 
                request.ArticleId, true, cancellationToken) ?? throw new CartItemNotFoundException(request.ArticleId);
        
        cartItem.Count = request.NewCount;
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}