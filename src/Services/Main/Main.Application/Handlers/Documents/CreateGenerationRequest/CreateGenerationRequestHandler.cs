using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Attributes;
using Main.Application.Lrts.GenerateDocument;
using Main.Entities.Documents;

namespace Main.Application.Handlers.Documents.CreateGenerationRequest;

[Transactional, AutoSave]
public record CreateGenerationRequestCommand(
	string DocumentSystemName,
	string DocumentRequest,
	Guid? RequesterId = null) : ICommand<CreateGenerationRequestResult>;

public record CreateGenerationRequestResult(Guid RequestId);

public class CreateGenerationRequestHandler(
	IUnitOfWork unitOfWork,
	IJobService jobService,
	IJobProvider<GenerateDocumentLrt, GenerateDocumentInputState> provider
	) : ICommandHandler<CreateGenerationRequestCommand, CreateGenerationRequestResult>
{
	public async Task<CreateGenerationRequestResult> Handle(
		CreateGenerationRequestCommand request,
		CancellationToken cancellationToken)
	{
		var jobs = await jobService.TryEnqueueJobsAsync(
			[
				provider.Create(
				new GenerateDocumentInputState
				{
					DocumentSystemName = request.DocumentSystemName,
					DocumentRequest = request.DocumentRequest
				})
			],
			cancellationToken);

		if (jobs.Count != 1)
			throw new InvalidOperationException("Unable to enqueue job for document generation.");

		var requestModel = DocumentGenerationRequest.Create(
			jobs[0],
			request.DocumentSystemName,
			request.RequesterId);

		await unitOfWork.AddAsync(requestModel, cancellationToken);

		return new CreateGenerationRequestResult(requestModel.RequestId);
	}
}
