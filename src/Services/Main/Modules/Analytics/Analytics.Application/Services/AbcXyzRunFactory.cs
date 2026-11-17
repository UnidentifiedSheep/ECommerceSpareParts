using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Models;

namespace Main.Modules.Analytics.Application.Services;

public static class AbcXyzRunFactory
{
	public const string AnalysisType = "AbcXyz";

	public const int SchemaVersion = 1;

	public static AbcXyzRunDraft Create(
		AbcXyzParameters parameters,
		AbcXyzAnalysisResult result,
		Guid? jobId,
		Guid? createdBy)
	{
		var parametersJson = SerializeParameters(parameters);
		var run = AnalyticsRun.Create(
			AnalysisType,
			SchemaVersion,
			parametersJson,
			Hash(parametersJson),
			parameters.From,
			parameters.EndExclusive(),
			jobId,
			createdBy);
		var items = result.Items.Select(item => CreateItem(run.Id, item)).ToList();
		run.Complete(AnalyticsJson.Serialize(result.Summary), items.Count);
		return new AbcXyzRunDraft(run, items);
	}

	public static string ComputeHash(AbcXyzParameters parameters) => Hash(SerializeParameters(parameters));

	private static string SerializeParameters(AbcXyzParameters parameters)
	{
		var months = parameters.Months?.Distinct().Order().ToArray();
		var normalized = new AbcXyzParameters
		{
			From = parameters.From,
			To = parameters.To,
			Months = months is { Length: > 0 } ? months : null,
			AbcMetric = parameters.AbcMetric,
			XyzMetric = parameters.XyzMetric,
			AShare = parameters.AShare,
			BShare = parameters.BShare,
			XCoefficient = parameters.XCoefficient,
			YCoefficient = parameters.YCoefficient,
			MinPeriods = parameters.MinPeriods,
			OrganizationId = parameters.OrganizationId,
			BuyerId = parameters.BuyerId
		};
		return AnalyticsJson.Serialize(normalized);
	}

	private static string Hash(string json)
		=> Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(json)));

	private static AnalyticsRunItem CreateItem(Guid runId, AbcXyzItem item)
		=> new AnalyticsRunItemModel<AbcXyzItemSegments, AbcXyzItemData>(
				item.ProductId.ToString(CultureInfo.InvariantCulture),
				item.Rank,
				item.Value,
				item.Share,
				new AbcXyzItemSegments(item.AbcClass, item.XyzClass),
				new AbcXyzItemData(
					item.Quantity,
					item.Revenue,
					item.CumulativeShare,
					item.CoefficientOfVariation,
					item.PeriodsCount))
			.ToEntity(runId);
}

public sealed record AbcXyzRunDraft(AnalyticsRun Run, IReadOnlyList<AnalyticsRunItem> Items);
