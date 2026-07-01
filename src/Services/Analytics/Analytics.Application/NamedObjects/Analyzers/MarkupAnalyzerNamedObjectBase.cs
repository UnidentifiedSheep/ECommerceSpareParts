using Analytics.Application.NamedObjects.Analyzers.Markup;
using Application.Common.Abstractions.NamedObjects;

namespace Analytics.Application.NamedObjects.Analyzers;

public abstract class MarkupAnalyzerNamedObjectBase : LocalizableNameObject
{
    public abstract Task<IReadOnlyList<MarkupRangeDraft>> AnalyzeAsync(
        MarkupAnalyzerInput input,
        CancellationToken cancellationToken = default);
}