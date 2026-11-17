namespace Main.Modules.Analytics.Abstractions.Analyzers.Markup;

public sealed record MarkupRangeDraft(
	decimal FromCost,
	decimal ToCost,
	decimal MeanMarkup,
	decimal StdDevMarkup,
	int Count);
