using FluentValidation;

namespace Main.Application.Handlers.ProductEans.GetProductsEans;

public class GetProductsEansValidation : AbstractValidator<GetProductsEansQuery>
{
	public GetProductsEansValidation()
	{
		RuleForEach(x => x.ProductIds).GreaterThan(0);
	}
}
