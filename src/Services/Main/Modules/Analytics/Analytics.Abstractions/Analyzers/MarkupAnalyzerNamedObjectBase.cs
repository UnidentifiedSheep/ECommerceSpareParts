using Main.Modules.Analytics.Abstractions.Analyzers.Markup;
using NamedObject.Core.Base;

namespace Main.Modules.Analytics.Abstractions.Analyzers;

public abstract class MarkupAnalyzerNamedObjectBase : LocalizableNameObject
{
	public abstract Task<IReadOnlyList<MarkupRangeDraft>> AnalyzeAsync(
		MarkupAnalyzerInput input,
		CancellationToken cancellationToken = default);
}
