using Locan.Core.Interfaces;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Analyses;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Charts.CoPurchaseRunItems;
using Main.Modules.Analytics.Application.Lrts.CoPurchaseCalculation;
using Main.Modules.Analytics.Application.Models.CoPurchase;
using SchemaGeneration.Abstractions;

namespace Main.Modules.Analytics.Application.Analyses;

public sealed class CoPurchaseAnalysis(ISchemaGenerator schemaGenerator)
	: AnalysisNamedObject<
		CoPurchaseParameters,
		CoPurchaseSummary,
		CoPurchaseItemSegments,
		CoPurchaseItemData>(schemaGenerator)
{
	public const string Name = "CoPurchase";

	public const int Version = 1;

	public override string SystemName => Name;

	public override string LrtSystemName => nameof(CoPurchaseCalculationLrt);

	public override string ItemsChartSystemName => CoPurchaseRunItemsChartDataSource.DataSourceSystemName;

	public override int SchemaVersion => Version;

	public override ILocalizableMessage NameLocalizationMessage => AnalysisCoPurchaseNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => AnalysisCoPurchaseDescriptionMessage.Instance;
}
