using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;
using Main.Entities.Product;

namespace Main.Application.Handlers.ProductGroups.CreateProductGroup;

public class CreateProductGroupDbValidation : AbstractDbValidation<CreateProductGroupCommand>
{
	public override void Build(IValidationPlan plan, CreateProductGroupCommand request) =>
		plan.ValidateProductGroupNotExistsNormalizedName(
			ProductGroup.NormalizeName(request.Name.Trim()));
}
