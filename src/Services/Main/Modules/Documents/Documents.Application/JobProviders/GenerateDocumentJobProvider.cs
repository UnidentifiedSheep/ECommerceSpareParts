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
			GenerateDocumentLrt.Name,
			JsonSerializer.Serialize(inputState),
			maxAttempts);
}
