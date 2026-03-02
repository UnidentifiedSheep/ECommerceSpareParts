using Lucene.Net.Search.Suggest;
using Lucene.Net.Util;
using Search.Entities;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Enumerators;

public sealed class ArticleEnumerator(IArticleReadRepository articleReadRepository) : IInputEnumerator, IDisposable
{
    public Article? CurrentArticle { get; private set; }
    public BytesRef Current => CurrentArticle == null 
        ? new BytesRef() 
        : new BytesRef($"{CurrentArticle.ArticleNumber} {CurrentArticle.Title}");
    public IComparer<BytesRef> Comparer => BytesRef.UTF8SortedAsUnicodeComparer;
    
    public long Weight => CurrentArticle?.Popularity ?? 1;

    public BytesRef Payload => CurrentArticle == null
        ? new BytesRef()
        : new BytesRef($"{CurrentArticle.Id}");

    public bool HasPayloads => true;
    public ICollection<BytesRef> Contexts => [];
    public bool HasContexts => false;
    
    private bool _started;

    public bool MoveNext()
    {
        if (!_started)
        {
            CurrentArticle = articleReadRepository.GetNextArticle();
            _started = true;
            return CurrentArticle != null;
        }

        if (CurrentArticle == null) return false;

        CurrentArticle = articleReadRepository.GetNextArticle(CurrentArticle.Id);
        return CurrentArticle != null;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}