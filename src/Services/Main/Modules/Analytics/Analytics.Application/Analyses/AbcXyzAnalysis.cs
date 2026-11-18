using Locan.Core.Interfaces;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Analyses;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Charts.AbcXyzRunItems;
using Main.Modules.Analytics.Application.Lrts.AbcXyzCalculation;
using Main.Modules.Analytics.Application.Models;
using SchemaGeneration.Abstractions;

namespace Main.Modules.Analytics.Application.Analyses;

public sealed class AbcXyzAnalysis(ISchemaGenerator schemaGenerator)
	: AnalysisNamedObject<AbcXyzParameters, AbcXyzSummary, AbcXyzItemSegments, AbcXyzItemData>(schemaGenerator)
{
	public const string Name = "AbcXyz";

	public const int Version = 2;

	public override string SystemName => Name;

	public override string LrtSystemName => nameof(AbcXyzCalculationLrt);

	public override string ItemsChartSystemName => AbcXyzRunItemsChartDataSource.DataSourceSystemName;

	public override int SchemaVersion => Version;

	public override ILocalizableMessage NameLocalizationMessage => AnalysisAbcXyzNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => AnalysisAbcXyzDescriptionMessage.Instance;
}
