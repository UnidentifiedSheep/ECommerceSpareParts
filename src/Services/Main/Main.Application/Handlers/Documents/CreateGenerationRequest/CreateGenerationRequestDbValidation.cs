using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Documents.CreateGenerationRequest;

public class CreateGenerationRequestDbValidation : AbstractDbValidation<CreateGenerationRequestCommand>
{
	public override void Build(IValidationPlan plan, CreateGenerationRequestCommand request)
	{
		if (request.RequesterId != null)
			plan.ValidateUserExistsId(request.RequesterId.Value);
	}
}
