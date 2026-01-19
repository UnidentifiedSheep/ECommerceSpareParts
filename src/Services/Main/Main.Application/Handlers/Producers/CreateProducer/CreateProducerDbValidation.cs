using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Producers.CreateProducer;

public class CreateProducerDbValidation : AbstractDbValidation<CreateProducerCommand>
{
    public override void Build(IValidationPlan plan, CreateProducerCommand request)
    {
        plan.ValidateProducerNotExistsName(request.NewProducer.ProducerName);
    }
}