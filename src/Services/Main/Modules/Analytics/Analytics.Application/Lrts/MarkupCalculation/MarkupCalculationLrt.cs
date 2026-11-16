using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Analyzers;
using Main.Modules.Analytics.Abstractions.Analyzers.Markup;
using Main.Modules.Analytics.Abstractions.Events;
using Microsoft.Extensions.Logging;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Analytics.Application.Lrts.MarkupCalculation;

public class MarkupCalculationLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<MarkupCalculationLrt> logger) : LrtBase<MarkupCalculationInputState, MarkupCalculationState>(
	transactionService,
	logger)
{
	public override string SystemName => nameof(MarkupCalculationLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtMarkupCalculationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LrtMarkupCalculationDescriptionMessage.Instance;

	protected override async Task DoWork(MarkupCalculationInputState inputState)
	{
		var ranges = await ExecuteInScopeAsync(
			async (context, ct) =>
			{
				var analyzer = context
					.GetRequiredService<INamedObjectRegistry<MarkupAnalyzerNamedObjectBase>>()
					.GetBySystemName(MarkupAnalyzerNames.Range);
				var result = await analyzer.AnalyzeAsync(
					new MarkupAnalyzerInput
					{
						StartDate = inputState.RangeStart, EndDate = inputState.RangeEnd
					},
					ct);
				return result.Select(x => new MarkupRangeItem
					{
						Count = x.Count,
						FromCost = x.FromCost,
						MeanMarkup = x.MeanMarkup,
						StdDevMarkup = x.StdDevMarkup,
						ToCost = x.ToCost
					})
					.ToList();
			},
			CancellationToken);

		await ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(20, 2),
			async (context, cancellationToken) =>
			{
				await context.Publisher.Publish(
					new MarkupAnalyzedEvent
					{
						Ranges = ranges
					},
					cancellationToken);

				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
			},
			CancellationToken);
	}
}
