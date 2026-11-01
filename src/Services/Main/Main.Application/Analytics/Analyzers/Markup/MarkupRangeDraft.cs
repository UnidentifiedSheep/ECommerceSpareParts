namespace Main.Application.Analytics.Analyzers.Markup;

public sealed record MarkupRangeDraft(
	decimal FromCost,
	decimal ToCost,
	decimal MeanMarkup,
	decimal StdDevMarkup,
	int Count);
