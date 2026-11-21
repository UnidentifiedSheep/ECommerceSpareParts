using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Product;
using Main.Entities.Product.Producer;

namespace Main.Modules.Product.Application.Commands.Producers.CreateProducer;

public class CreateProducerDbValidation : AbstractDbValidation<CreateProducerCommand>
{
	public override void Build(IValidationPlan plan, CreateProducerCommand request)
		=> plan.ValidateProducerNotExistsName(Producer.ToNormalizedName(request.NewProducer.Name));
}
