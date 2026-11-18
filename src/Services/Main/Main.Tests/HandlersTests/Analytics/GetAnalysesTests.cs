using FluentAssertions;
using Main.Modules.Analytics.Abstractions.Analyses;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Analyses;
using Main.Modules.Analytics.Application.Charts.AbcXyzRunItems;
using Main.Modules.Analytics.Application.Queries;
using Main.Modules.Analytics.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using NamedObject.Core.Interfaces;
using SchemaGeneration.Abstractions.Models;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Analytics;

public class GetAnalysesTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task GetAnalyses_ReturnsAbcXyzWithLocalizedNameLrtAndSchemas()
	{
		var result = await Mediator.Send(new GetAnalysesQuery(), CancellationToken);

		var analysis = result.Analyses.Should().ContainSingle(item => item.SystemName == AbcXyzAnalysis.Name).Subject;
		analysis.Name.Should().NotStartWith("analysis.");
		analysis.Description.Should().NotStartWith("analysis.");
		analysis.LrtSystemName.Should().Be("AbcXyzCalculationLrt");
		analysis.ItemsChartSystemName.Should().Be(AbcXyzRunItemsChartDataSource.DataSourceSystemName);
		analysis.SchemaVersion.Should().Be(AbcXyzRunFactory.SchemaVersion);
		analysis.ParametersSchema.Fields.Select(field => field.Name)
			.Should()
			.Contain(["from", "to", "abcMetric", "xyzMetric", "minPeriods"]);
		analysis.SummarySchema.Fields.Select(field => field.Name)
			.Should()
			.BeEquivalentTo("productsCount", "periodsCount", "totalValue", "cells", "patternCells");
		analysis.SummarySchema.Fields.Single(field => field.Name == "cells").NestedSchema!.Fields
			.Select(field => field.Name)
			.Should()
			.BeEquivalentTo("abc", "xyz", "productsCount", "value");
		analysis.SegmentsSchema.Fields.Select(field => field.Name).Should().BeEquivalentTo("abc", "xyz", "demandPattern");
		analysis.DataSchema.Fields.Select(field => field.Name)
			.Should()
			.BeEquivalentTo("quantity", "revenue", "cumulativeShare", "coefficientOfVariation", "periodsCount", "adi", "cv2");
	}

	[Fact]
	public async Task GetAnalyses_LocalizesEveryOutputField()
	{
		var result = await Mediator.Send(new GetAnalysesQuery(), CancellationToken);

		var analysis = result.Analyses.Single(item => item.SystemName == AbcXyzAnalysis.Name);
		foreach (var schema in new[] { analysis.SummarySchema, analysis.SegmentsSchema, analysis.DataSchema })
			AssertLocalized(schema);
		AssertLocalized(analysis.SummarySchema.Fields.Single(field => field.Name == "cells").NestedSchema!);
		AssertLocalized(analysis.SummarySchema.Fields.Single(field => field.Name == "patternCells").NestedSchema!);
	}

	[Fact]
	public async Task AnalysisItemsChart_IsAnExistingRegisteredChart()
	{
		var analyses = await Mediator.Send(new GetAnalysesQuery(), CancellationToken);
		var charts = await Mediator.Send(new GetChartsQuery(), CancellationToken);

		charts.Charts.Select(chart => chart.SystemName)
			.Should()
			.Contain(analyses.Analyses.Select(analysis => analysis.ItemsChartSystemName));
	}

	[Fact]
	public async Task AbcXyzItemsChart_DeclaresProductIdAsProductReference()
	{
		var charts = await Mediator.Send(new GetChartsQuery(), CancellationToken);

		var field = charts.Charts
			.Single(chart => chart.SystemName == AbcXyzRunItemsChartDataSource.DataSourceSystemName)
			.DataPointSchema.Fields.Single(field => field.Name == "productId");
		field.Dependency.Should().NotBeNull();
		field.Dependency!.EntityName.Should().Be("Product");
		field.Dependency.FieldName.Should().Be("id");
	}

	[Fact]
	public async Task AbcXyzItemsChart_DeclaresEveryPointReferenceInSchema()
	{
		var charts = await Mediator.Send(new GetChartsQuery(), CancellationToken);
		var schema = charts.Charts
			.Single(chart => chart.SystemName == AbcXyzRunItemsChartDataSource.DataSourceSystemName)
			.DataPointSchema;
		var point = new AbcXyzRunItemDataPoint
		{
			ProductId = 1,
			Rank = 1,
			Value = 1m,
			Share = 1m,
			CumulativeShare = 1m,
			Abc = AbcClass.A,
			Quantity = 1m,
			Revenue = 1m,
			PeriodsCount = 1
		};

		var references = point.GetReferences();

		references.Should().NotBeEmpty();
		foreach (var reference in references)
			schema.Fields.Should()
				.Contain(field => field.Name == reference.Field
					&& field.Dependency != null
					&& field.Dependency.EntityName == reference.Entity.ToString());
	}

	[Fact]
	public void AnalysisGroup_IsRegisteredWithSystemNameAsRunAnalysisType()
	{
		var registry = Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<AnalysisNamedObject>>();

		registry.TryGetBySystemName(AbcXyzRunFactory.AnalysisType).Should().NotBeNull();
	}

	private static void AssertLocalized(ObjectSchema schema)
	{
		schema.Fields.Should().OnlyContain(field => !string.IsNullOrWhiteSpace(field.Label));
		schema.Fields.Should().OnlyContain(field => !string.IsNullOrWhiteSpace(field.Description));
		schema.Fields.Should().OnlyContain(field => !field.Label!.StartsWith("abc.xyz."));
		schema.Fields.Should().OnlyContain(field => !field.Description!.StartsWith("abc.xyz."));
	}
}
