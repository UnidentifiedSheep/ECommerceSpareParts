using Lucene.Net.Search.Suggest;
using Lucene.Net.Util;
using Search.Entities;

namespace Search.Persistence.Enumerators;

internal sealed class ArticleInputEnumerator(IEnumerable<Article> articles) : IInputEnumerator
{
    private readonly IEnumerator<Article> _enumerator = articles.GetEnumerator();

    private Article? _currentArticle;

    public bool HasPayloads => true;
    public ICollection<BytesRef> Contexts { get; } = [];
    public bool HasContexts => false;

    public BytesRef Payload
    {
        get
        {
            if (_currentArticle == null) return new BytesRef();
            return new(_currentArticle.Id);
        }
    }
    public long Weight => _currentArticle?.Popularity ?? 1;

    public IComparer<BytesRef> Comparer => BytesRef.UTF8SortedAsUnicodeComparer;

    public BytesRef Current
    {
        get
        {
            if (_currentArticle == null) return new BytesRef();
            return new($"{_currentArticle.ArticleNumber} {_currentArticle.Title}");
        }
    }

    public bool MoveNext()
    {
        var hasNext = _enumerator.MoveNext();
        if (hasNext)
        {
            _currentArticle = _enumerator.Current;
        }
        return hasNext;
    }
}