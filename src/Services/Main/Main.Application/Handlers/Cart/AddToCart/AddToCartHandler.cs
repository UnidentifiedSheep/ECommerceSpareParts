using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Cart;
using Exceptions.Exceptions.Users;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Cart.AddToCart;

[Transactional]
[ExceptionType<UserNotFoundException>]
[ExceptionType<SameItemInCartException>]
public record AddToCartCommand(Guid UserId, int ArticleId, int Count) : ICommand;

public class AddToCartHandler(ICartRepository cartRepository, IUserRepository userRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<AddToCartCommand>
{
    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        await ValidateData(request.UserId, request.ArticleId, cancellationToken);

        var cartItem = new Core.Entities.Cart
        {
            UserId = request.UserId,
            ArticleId = request.ArticleId,
            Count = request.Count
        };
        await unitOfWork.AddAsync(cartItem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(Guid userId, int articleId, CancellationToken cancellationToken)
    {
        if (await cartRepository.GetCartItemAsync(userId, articleId, false, cancellationToken) != null)
            throw new SameItemInCartException(articleId);
        if (await userRepository.GetUserByIdAsync(userId, false, cancellationToken) == null)
            throw new UserNotFoundException(userId);
    }
}