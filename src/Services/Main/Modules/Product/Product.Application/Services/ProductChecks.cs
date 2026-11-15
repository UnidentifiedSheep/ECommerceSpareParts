using BulkValidation.Core.Interfaces;
using Main.Entities.Product;
using Main.Modules.Product.Abstractions;

namespace Main.Application.Services;

public sealed class ProductChecks : IProductChecks
{
	public IValidationPlan Exists(IValidationPlan plan, int productId) => plan.ValidateProductExistsId(productId);

	public IValidationPlan Exists(IValidationPlan plan, IEnumerable<int> productIds)
		=> plan.ValidateProductExistsId(productIds);
}
