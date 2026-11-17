using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Cart.Exceptions;
using MediatR;

namespace Main.Modules.Cart.Application.Handlers.Cart.ChangeCartItemCount;

[Transactional]
public record ChangeCartItemCountCommand(Guid UserId, int ProductId, int NewCount) : ICommand;

public class ChangeCartItemCountHandler(IRepository<Entities.Cart.Cart.Cart, (Guid, int)> repository)
	: ICommandHandler<ChangeCartItemCountCommand>
{
	public async Task<Unit> Handle(ChangeCartItemCountCommand request, CancellationToken cancellationToken)
	{
		var cartItem = await repository.GetById((request.UserId, request.ProductId), cancellationToken) ??
			throw new CartItemNotFoundException(request.ProductId);

		cartItem.SetCount(request.NewCount);

		return Unit.Value;
	}
}
