using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Modules.Identity.Abstractions.Interfaces;

namespace Main.Modules.Documents.Application.Commands.Documents.CreateGenerationRequest;

public class CreateGenerationRequestDbValidation(IUserChecks userChecks)
	: AbstractDbValidation<CreateGenerationRequestCommand>
{
	public override void Build(IValidationPlan plan, CreateGenerationRequestCommand request)
	{
		if (request.RequesterId != null)
			userChecks.Exists(plan, request.RequesterId.Value);
	}
}
