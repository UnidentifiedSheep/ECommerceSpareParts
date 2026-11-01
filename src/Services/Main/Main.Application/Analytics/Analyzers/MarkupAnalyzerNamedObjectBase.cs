using Main.Application.Analytics.Analyzers.Markup;
using NamedObject.Core.Base;

namespace Main.Application.Analytics.Analyzers;

public abstract class MarkupAnalyzerNamedObjectBase : LocalizableNameObject
{
	public abstract Task<IReadOnlyList<MarkupRangeDraft>> AnalyzeAsync(
		MarkupAnalyzerInput input,
		CancellationToken cancellationToken = default);
}
