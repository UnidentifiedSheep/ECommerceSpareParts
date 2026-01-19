using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Cart.AddToCart;

public class AddToCartDbValidation : AbstractDbValidation<AddToCartCommand>
{
    public override void Build(IValidationPlan plan, AddToCartCommand request)
    {
        plan.ValidateUserExistsId(request.UserId)
            .ValidateCartNotExistsPK((request.UserId, request.ArticleId));
    }
}