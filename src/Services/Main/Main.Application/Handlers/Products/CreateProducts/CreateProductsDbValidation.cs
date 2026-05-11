using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Products.CreateProducts;

public class CreateProductsDbValidation : AbstractDbValidation<CreateProductsCommand>
{
    public override void Build(IValidationPlan plan, CreateProductsCommand request)
    {
        plan.ValidateProducerExistsId(request.NewProducts.Select(x => x.ProducerId));
    }
}