using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Domain.CommonEntities.Job;
using Main.Modules.Documents.Application.Lrts.GenerateDocument;

namespace Main.Modules.Documents.Application.JobProviders;

[Lifetime(Lifetime.Singleton)]
public class GenerateDocumentJobProvider : IJobProvider<GenerateDocumentLrt, GenerateDocumentInputState>
{
	public Job Create(GenerateDocumentInputState inputState, int maxAttempts = 3)
		=> SingleRunJob.Create(
			GenerateDocumentLrt.Name,
			JsonSerializer.Serialize(inputState),
			maxAttempts);
}
