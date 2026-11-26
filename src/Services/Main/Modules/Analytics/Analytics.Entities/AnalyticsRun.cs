using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Analytics;

public class AnalyticsRun : Entity<AnalyticsRun, Guid>, ILinqEntity<AnalyticsRun, Guid>
{
	private readonly List<AnalyticsRunItem> _items = [];

	private AnalyticsRun()
	{
	}

	public Guid Id { get; private set; }

	public string AnalysisType { get; private set; } = null!;

	public int SchemaVersion { get; private set; }

	public string Parameters { get; private set; } = null!;

	public string ParametersHash { get; private set; } = null!;

	public DateTime? PeriodStart { get; private set; }

	public DateTime? PeriodEnd { get; private set; }

	public string? Summary { get; private set; }

	public int ItemsCount { get; private set; }

	public Guid? JobId { get; private set; }

	public Guid? CreatedBy { get; private set; }

	public bool IsPinned { get; private set; }

	public DateTime CreatedAt { get; private set; }

	public DateTime? CompletedAt { get; private set; }

	public IReadOnlyCollection<AnalyticsRunItem> Items => _items;

	public static Expression<Func<AnalyticsRun, Guid>> GetKeySelector() => item => item.Id;

	public static Expression<Func<AnalyticsRun, bool>> GetEqualityExpression(Guid key)
		=> item => item.Id == key;

	public static AnalyticsRun Create(
		string analysisType,
		int schemaVersion,
		string parameters,
		string parametersHash,
		DateTime? periodStart,
		DateTime? periodEnd,
		Guid? jobId,
		Guid? createdBy)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(analysisType);
		ArgumentException.ThrowIfNullOrWhiteSpace(parameters);
		ArgumentException.ThrowIfNullOrWhiteSpace(parametersHash);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schemaVersion);
		if (periodStart > periodEnd)
			throw new ArgumentException("Period start cannot be later than period end.", nameof(periodStart));
		if (createdBy == Guid.Empty)
			throw new ArgumentException("Created by cannot be empty.", nameof(createdBy));

		return new AnalyticsRun
		{
			Id = Guid.NewGuid(),
			AnalysisType = analysisType.Trim(),
			SchemaVersion = schemaVersion,
			Parameters = parameters,
			ParametersHash = parametersHash.Trim(),
			PeriodStart = periodStart,
			PeriodEnd = periodEnd,
			JobId = jobId,
			CreatedBy = createdBy,
			CreatedAt = DateTime.UtcNow
		};
	}

	public void Complete(string? summary, int itemsCount)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(itemsCount);
		if (CompletedAt is not null)
			throw new InvalidOperationException("Analytics run is already completed.");
		Summary = summary;
		ItemsCount = itemsCount;
		CompletedAt = DateTime.UtcNow;
	}

	public void Pin() => IsPinned = true;

	public void Unpin() => IsPinned = false;

	public override Guid GetId() => Id;
}
