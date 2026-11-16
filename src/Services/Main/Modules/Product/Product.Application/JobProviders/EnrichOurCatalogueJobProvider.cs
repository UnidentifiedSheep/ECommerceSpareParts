using System.Text.Json;
using Application.Common;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Domain.CommonEntities.Job;
using Main.Modules.Product.Application.Lrts.EnrichCatalogue;

namespace Main.Modules.Product.Application.JobProviders;

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
