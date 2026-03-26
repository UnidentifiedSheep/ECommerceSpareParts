using Lucene.Net.Analysis;

namespace Search.Persistence.Analyzers;

public sealed class ArticleAnalyzer(Analyzer defaultAnalyzer, Analyzer titleFieldAnalyzer)
    : AnalyzerWrapper(PER_FIELD_REUSE_STRATEGY)
{
    private readonly Dictionary<string, Analyzer> _fieldAnalyzers = new() { { "Title", titleFieldAnalyzer } };

    protected override Analyzer GetWrappedAnalyzer(string fieldName)
    {
        _fieldAnalyzers.TryGetValue(fieldName, out var analyzer);
        return analyzer ?? defaultAnalyzer;
    }
}