using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Application.Analyses;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Models;
using Main.Modules.Analytics.Application.Models.CoPurchase;

namespace Main.Modules.Analytics.Application.Services;

public static class CoPurchaseRunFactory
{
	public const string AnalysisType = CoPurchaseAnalysis.Name;

	public const int SchemaVersion = CoPurchaseAnalysis.Version;

	public static AnalyticsRun CreateRun(CoPurchaseParameters parameters, Guid? jobId, Guid? createdBy)
	{
		var parametersJson = SerializeParameters(parameters);
		return AnalyticsRun.Create(
			AnalysisType,
			SchemaVersion,
			parametersJson,
			Hash(parametersJson),
			parameters.From,
			parameters.EndExclusive(),
			jobId,
			createdBy);
	}

	public static AnalyticsRunItem CreateItem(Guid runId, CoPurchaseProductResult result)
		=> new AnalyticsRunItemModel<CoPurchaseItemSegments, CoPurchaseItemData>(
				result.ProductId.ToString(CultureInfo.InvariantCulture),
				result.ProductId,
				result.SalesCount,
				null,
				new CoPurchaseItemSegments(result.Related.Count > 0),
				new CoPurchaseItemData(result.SalesCount, result.Related))
			.ToEntity(runId);

	public static string SerializeSummary(CoPurchaseSummary summary) => AnalyticsJson.Serialize(summary);

	public static string ComputeHash(CoPurchaseParameters parameters) => Hash(SerializeParameters(parameters));

	private static string SerializeParameters(CoPurchaseParameters parameters)
		=> AnalyticsJson.Serialize(
			new CoPurchaseParameters
			{
				From = parameters.From,
				To = parameters.To,
				MinPairCount = parameters.MinPairCount,
				SuspiciousCrossMinPairCount = parameters.SuspiciousCrossMinPairCount,
				SuspiciousCrossMinLift = parameters.SuspiciousCrossMinLift,
				CandidatesTopK = parameters.CandidatesTopK,
				ResultTopN = parameters.ResultTopN
			});

	private static string Hash(string json)
		=> Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
}
