using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions.Cart;
using MediatR;

namespace Main.Application.Handlers.Cart.DeleteFromCart;

[AutoSave]
[Transactional]
public record DeleteFromCartCommand(Guid UserId, int ProductId) : ICommand;

public class DeleteFromCartHandler(
    IRepository<Entities.Cart.Cart, (Guid, int)> repository, 
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteFromCartCommand>
{
    public async Task<Unit> Handle(DeleteFromCartCommand request, CancellationToken cancellationToken)
    {
        var cartItem =
            await repository.GetById((request.UserId, request.ProductId), cancellationToken) ??
            throw new CartItemNotFoundException(request.ProductId);

        unitOfWork.Remove(cartItem);
        return Unit.Value;
    }
}