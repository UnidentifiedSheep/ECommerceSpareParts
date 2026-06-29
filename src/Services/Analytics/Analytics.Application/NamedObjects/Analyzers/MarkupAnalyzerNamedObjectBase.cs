using Application.Common.Abstractions.NamedObjects;

namespace Analytics.Application.NamedObjects.Analyzers.Markup;

public abstract class MarkupAnalyzerNamedObjectBase : LocalizableNameObject
{
    public abstract Task<IReadOnlyList<MarkupRangeDraft>> AnalyzeAsync(
        MarkupAnalyzerInput input,
        CancellationToken cancellationToken = default);
}