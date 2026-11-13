using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Product;

namespace Main.Application.Handlers.Cart.AddToCart;

public class AddToCartDbValidation : AbstractDbValidation<AddToCartCommand>
{
	public override void Build(IValidationPlan plan, AddToCartCommand request)
		=> plan
			.ValidateUserExistsId(request.UserId)
			.ValidateProductExistsId(request.ProductId)
			.ValidateCartNotExistsPK((request.UserId, request.ProductId));
}
