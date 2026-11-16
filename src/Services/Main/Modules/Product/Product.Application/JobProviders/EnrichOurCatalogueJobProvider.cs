using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using Main.Application.Lrts.EnrichCatalogue;

namespace Main.Application.JobProviders;

[Lifetime(Lifetime.Scoped)]
public sealed class EnrichOurCatalogueJobProvider(
	IJobCreationDispatcher creationDispatcher
) : IJobProvider<EnrichOurCatalogueLrt, NoneInputState>
{
	public Job Create(NoneInputState inputState, int maxAttempts = 3)
		=> creationDispatcher.Create(
			EnrichOurCatalogueLrt.Name,
			JsonSerializer.Serialize(inputState),
			maxAttempts,
			"enrich:our:catalogue:job");
}
