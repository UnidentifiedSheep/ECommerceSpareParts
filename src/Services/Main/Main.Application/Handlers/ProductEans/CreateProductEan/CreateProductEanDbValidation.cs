using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.ProductEans.CreateProductEan;

public class CreateProductEanDbValidation : AbstractDbValidation<CreateProductEanCommand>
{
	public override void Build(IValidationPlan plan, CreateProductEanCommand request) =>
		plan.ValidateProductExistsId(request.ProductId);
}
