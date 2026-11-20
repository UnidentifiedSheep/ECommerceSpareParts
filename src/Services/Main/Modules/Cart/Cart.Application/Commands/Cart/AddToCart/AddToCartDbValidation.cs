using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Cart;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Product.Abstractions;

namespace Main.Modules.Cart.Application.Commands.Cart.AddToCart;

public class AddToCartDbValidation(IUserChecks userChecks, IProductChecks productChecks)
	: AbstractDbValidation<AddToCartCommand>
{
	public override void Build(IValidationPlan plan, AddToCartCommand request)
	{
		userChecks.Exists(plan, request.UserId);
		productChecks.Exists(plan, request.ProductId);
		plan.ValidateCartNotExistsPK((request.UserId, request.ProductId));
	}
}
