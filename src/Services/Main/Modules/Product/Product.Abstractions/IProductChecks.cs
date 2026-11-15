using BulkValidation.Core.Interfaces;

namespace Main.Modules.Product.Abstractions;

public interface IProductChecks
{
	IValidationPlan Exists(IValidationPlan plan, int productId);

	IValidationPlan Exists(IValidationPlan plan, IEnumerable<int> productIds);
}
