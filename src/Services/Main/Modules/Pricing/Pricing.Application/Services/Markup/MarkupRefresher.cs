using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Application.Common.LRT;
using Attributes;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;

namespace Pricing.Application.Services.Markup;

public class MarkupRefresher(
	IMarkupInitializer markupInitializer,
	IApplicationTransaction transactionService,
	IJobService jobService,
	IJobProvider<InvalidateStalePriceOptionsLrt, NoneInputState> jobProvider) : IMarkupRefresher
{
	public async Task Refresh(CancellationToken cancellationToken)
	{
		await markupInitializer.Initialize(cancellationToken);

		await transactionService.WithSettingsIfIdle(TransactionalAttribute.ReadCommitted(30, 3))
			.ExecuteAsync(
				async (transactionContext, ct) =>
				{
					var job = jobProvider.Create(new NoneInputState());
					var addedIds = await jobService.TryEnqueueJobsAsync([job], ct);
					if (addedIds.Count != 0)
						await transactionContext.UnitOfWork.SaveChangesAsync(ct);
				},
				cancellationToken);
	}
}
