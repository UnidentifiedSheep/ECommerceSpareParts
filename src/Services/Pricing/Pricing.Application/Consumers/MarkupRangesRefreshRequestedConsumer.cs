using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Application.Common.LRT;
using Attributes;
using Contracts.Analytics;
using MassTransit;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;

namespace Pricing.Application.Consumers;

public class MarkupRangesRefreshRequestedConsumer(
	IMarkupInitializer markupInitializer,
	IApplicationTransactionService transactionService,
	IJobService jobService,
	IJobProvider<InvalidateStalePriceOptionsLrt, NoneInputState> jobProvider)
	: IConsumer<MarkupRangesRefreshRequestedEvent>
{
	public async Task Consume(ConsumeContext<MarkupRangesRefreshRequestedEvent> context)
	{
		await markupInitializer.Initialize(context.CancellationToken);

		await transactionService.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			async (transactionContext, ct) =>
			{
				var job = jobProvider.Create(new NoneInputState());
				var addedIds = await jobService.TryEnqueueJobsAsync([job], ct);
				if (addedIds.Count != 0)
					await transactionContext.UnitOfWork.SaveChangesAsync(ct);
			},
			context.CancellationToken);
	}
}
