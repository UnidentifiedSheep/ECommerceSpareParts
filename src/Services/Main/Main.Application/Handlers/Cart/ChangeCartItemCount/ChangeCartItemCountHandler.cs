using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Cart;
using Exceptions.Exceptions.Users;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Cart.ChangeCartItemCount;

[Transactional]
public record ChangeCartItemCountCommand(Guid UserId, int ArticleId, int NewCount) : ICommand;

public class ChangeCartItemCountHandler(ICartRepository cartRepository, IUserRepository userRepository, IUnitOfWork unitOfWork) : ICommandHandler<ChangeCartItemCountCommand>
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