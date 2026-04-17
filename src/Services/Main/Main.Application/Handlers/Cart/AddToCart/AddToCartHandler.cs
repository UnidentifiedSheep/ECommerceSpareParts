using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using MediatR;

namespace Main.Application.Handlers.Cart.AddToCart;

[AutoSave]
[Transactional]
public record AddToCartCommand(Guid UserId, int ProductId, int Count) : ICommand;

public class AddToCartHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddToCartCommand>
{
    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = Entities.Cart.Cart.Create(request.UserId, request.ProductId, request.Count);
        await unitOfWork.AddAsync(cart, cancellationToken);
        return Unit.Value;
    }
}