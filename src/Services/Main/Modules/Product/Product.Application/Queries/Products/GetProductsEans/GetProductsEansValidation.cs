using FluentValidation;

namespace Main.Modules.Product.Application.Queries.Products.GetProductsEans;

public class GetProductsEansValidation : AbstractValidator<GetProductsEansQuery>
{
	public GetProductsEansValidation() => RuleForEach(x => x.ProductIds).GreaterThan(0);
}
