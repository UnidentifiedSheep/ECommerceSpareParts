using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Domain.CommonEntities.Job;
using Main.Application.Lrts.GenerateDocument;

namespace Main.Application.JobProviders;

[Lifetime(Lifetime.Singleton)]
public class GenerateDocumentJobProvider : IJobProvider<GenerateDocumentLrt, GenerateDocumentInputState>
{
	public Job Create(GenerateDocumentInputState inputState, int maxAttempts = 3)
		=> SingleRunJob.Create(
			systemName: GenerateDocumentLrt.Name,
			initialState: JsonSerializer.Serialize(inputState),
			maxAttempts: maxAttempts);
}
