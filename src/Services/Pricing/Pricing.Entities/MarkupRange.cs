namespace Pricing.Entities;

public class MarkupRange
{
	private MarkupRange()
	{
	}

	private MarkupRange(
		decimal rangeStart,
		decimal rangeEnd,
		decimal markup)
	{
		RangeStart = rangeStart;
		RangeEnd = rangeEnd;
		Markup = markup;
	}

	public int Id { get; private set; }

	public decimal RangeStart { get; private set; }

	public decimal RangeEnd { get; private set; }

	public decimal Markup { get; private set; }

	public int GroupId { get; private set; }

	public MarkupGroup Group { get; private set; } = null!;

	public static bool HasOverlaps(IEnumerable<(decimal Start, decimal End)> ranges)
	{
		var ordered = ranges.OrderBy(range => range.Start).ThenBy(range => range.End).ToArray();
		for (var index = 1; index < ordered.Length; index++)
		{
			var previous = ordered[index - 1];
			var current = ordered[index];
			if (current.Start < previous.End || current.Start == previous.Start)
				return true;
		}

		return false;
	}

	public static MarkupRange Create(
		decimal rangeStart,
		decimal rangeEnd,
		decimal markup)
	{
		return new MarkupRange(
			rangeStart,
			rangeEnd,
			markup);
	}
}
