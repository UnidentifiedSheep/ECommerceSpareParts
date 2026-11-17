using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities.Product;
using Main.Entities.Product.Producer;

namespace Main.Modules.Product.Application.Handlers.ProducerAliases.AddAlias;

public class AddAliasDbValidation : AbstractDbValidation<AddAliasCommand>
{
	public override void Build(IValidationPlan plan, AddAliasCommand request)
		=> plan
			.ValidateProducerExistsId(request.ProducerId)
			.ValidateProducerAliasNotExistsAlias(Producer.ToNormalizedName(request.Alias));
}
