using Analytics.Application.NamedObjects.Analyzers.Markup;
using NamedObject.Core.Base;

namespace Analytics.Application.NamedObjects.Analyzers;

public abstract class MarkupAnalyzerNamedObjectBase : LocalizableNameObject
{
	public abstract Task<IReadOnlyList<MarkupRangeDraft>> AnalyzeAsync(
		MarkupAnalyzerInput input,
		CancellationToken cancellationToken = default);
}
