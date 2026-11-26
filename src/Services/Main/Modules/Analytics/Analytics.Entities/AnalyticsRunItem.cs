using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Analytics;

public class AnalyticsRunItem : Entity<AnalyticsRunItem, (Guid, string)>,
	ILinqEntity<AnalyticsRunItem, (Guid, string)>
{
	private AnalyticsRunItem()
	{
	}

	public Guid RunId { get; private set; }

	public string SubjectKey { get; private set; } = null!;

	public int Rank { get; private set; }

	public decimal? Value { get; private set; }

	public decimal? Share { get; private set; }

	public string Segments { get; private set; } = "{}";

	public string Data { get; private set; } = "{}";

	public AnalyticsRun Run { get; private set; } = null!;

	public static Expression<Func<AnalyticsRunItem, (Guid, string)>> GetKeySelector()
		=> item => ValueTuple.Create(item.RunId, item.SubjectKey);

	public static Expression<Func<AnalyticsRunItem, bool>> GetEqualityExpression((Guid, string) key)
		=> item => item.RunId == key.Item1 && item.SubjectKey == key.Item2;

	public static AnalyticsRunItem Create(
		Guid runId,
		string subjectKey,
		int rank,
		decimal? value,
		decimal? share,
		string? segments,
		string? data)
	{
		if (runId == Guid.Empty)
			throw new ArgumentException("Run id cannot be empty.", nameof(runId));
		ArgumentException.ThrowIfNullOrWhiteSpace(subjectKey);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rank);
		if (share is not null)
			ArgumentOutOfRangeException.ThrowIfNegative(share.Value);

		return new AnalyticsRunItem
		{
			RunId = runId,
			SubjectKey = subjectKey.Trim(),
			Rank = rank,
			Value = value,
			Share = share,
			Segments = string.IsNullOrWhiteSpace(segments) ? "{}" : segments,
			Data = string.IsNullOrWhiteSpace(data) ? "{}" : data
		};
	}

	public override (Guid, string) GetId() => (RunId, SubjectKey);
}
