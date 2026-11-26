using Domain.Validation;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Charts;
using Main.Modules.Analytics.Application.Services;
using SchemaGeneration.Abstractions;

namespace Main.Modules.Analytics.Application.Charts.CoPurchaseRelated;

public sealed class CoPurchaseRelatedChartDataSource(
	ICoPurchaseRelatedProvider relatedProvider,
	ISchemaGenerator schemaGenerator)
	: ChartDataSourceNamedObject<CoPurchaseRelatedDataPoint, CoPurchaseRelatedChartQuery>(schemaGenerator)
{
	public const string DataSourceSystemName = nameof(CoPurchaseRelatedChartDataSource);

	public override string SystemName => DataSourceSystemName;

	public override ILocalizableMessage NameLocalizationMessage => ChartCoPurchaseRelatedNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> ChartCoPurchaseRelatedDescriptionMessage.Instance;

	public override async Task<ChartDataResult<CoPurchaseRelatedDataPoint>> QueryAsync(
		CoPurchaseRelatedChartQuery queryInput,
		CancellationToken cancellationToken)
	{
		queryInput.EnsureNotNull(ChartCoPurchaseRelatedQueryRequiredMessage.Instance).Validate();
		var lookup = await relatedProvider.GetAsync(queryInput.ProductId, queryInput.RunId, cancellationToken);
		if (lookup is null)
			return new ChartDataResult<CoPurchaseRelatedDataPoint>([], null);

		var points = lookup.Related
			.Select((related, index) => new CoPurchaseRelatedDataPoint
			{
				Position = index + 1,
				ProductId = related.Members[0].ProductId,
				VariantProductIds = related.Members.Select(member => member.ProductId).ToList(),
				PairCount = related.PairCount,
				Confidence = related.Confidence,
				Lift = related.Lift,
				SourceProductId = lookup.SourceProductId
			})
			.ToList();
		return new ChartDataResult<CoPurchaseRelatedDataPoint>(points, null);
	}
}
