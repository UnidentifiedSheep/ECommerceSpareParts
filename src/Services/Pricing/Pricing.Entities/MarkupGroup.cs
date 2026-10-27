using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Validation;
using Pricing.Entities.DomainEvents;

namespace Pricing.Entities;

public class MarkupGroup : AuditableEntity<MarkupGroup, int>
{
	private readonly List<MarkupRange> _markupRanges = [];

	private MarkupGroup()
	{
	}

	private MarkupGroup(
		string? name,
		int currencyId,
		IEnumerable<MarkupRange> markupRanges,
		Guid? sourceMessageId = null)
	{
		Name = name;
		CurrencyId = currencyId;
		if (sourceMessageId == Guid.Empty)
			throw new ArgumentException("SourceMessageId must not be empty.", nameof(sourceMessageId));
		SourceMessageId = sourceMessageId;
		ReplaceRanges(markupRanges);
	}

	[Validate]
	public int Id { get; private set; }

	public string? Name { get; private set; }

	public Guid? SourceMessageId { get; private set; }

	public int CurrencyId { get; private set; }

	public IReadOnlyCollection<MarkupRange> MarkupRanges => _markupRanges;

	public static MarkupGroup Create(
		string? name,
		int currencyId,
		IEnumerable<MarkupRange> markupRanges,
		Guid? sourceMessageId = null)
	{
		return new MarkupGroup(
			name,
			currencyId,
			markupRanges,
			sourceMessageId);
	}

	public static MarkupGroup CreateGenerated(
		int currencyId,
		IEnumerable<MarkupRange> markupRanges,
		Guid sourceMessageId)
	{
		return new MarkupGroup(
			"Auto Generated Markup",
			currencyId,
			markupRanges,
			sourceMessageId);
	}

	public void SetCurrencyId(int currencyId) => CurrencyId = currencyId;

	public void SetName(string? name) => Name = name.TrimOrNull();

	public void Update(
		string? name,
		int currencyId,
		IEnumerable<MarkupRange> markupRanges)
	{
		SetName(name);
		SetCurrencyId(currencyId);
		ReplaceRanges(markupRanges);
	}

	public void ReplaceRanges(IEnumerable<MarkupRange> markupRanges)
	{
		ArgumentNullException.ThrowIfNull(markupRanges);
		var ranges = markupRanges.ToList();
		ranges.EnsureNotEmpty(() => new InvalidOperationException("Markup ranges are not empty"));

		_markupRanges.Clear();
		_markupRanges.AddRange(ranges);
		AddDomainEvent(
			new MarkupGroupUpdatedDomainEvent
			{
				Id = Id
			});
	}

	public override void OnUpdated() => AddDomainEvent(
		new MarkupGroupUpdatedDomainEvent
		{
			Id = Id
		});

	public override int GetId() => Id;
}
