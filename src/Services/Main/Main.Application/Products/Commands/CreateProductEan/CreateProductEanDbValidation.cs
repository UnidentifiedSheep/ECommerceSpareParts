using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Products.Commands.CreateProductEan;

public class CreateProductEanDbValidation : AbstractDbValidation<CreateProductEanCommand>
{
	public override void Build(IValidationPlan plan, CreateProductEanCommand request) =>
		plan.ValidateProductExistsId(request.ProductId)
			.ValidateProductEanNotExistsPK((request.ProductId, request.Ean.Trim()));
}
