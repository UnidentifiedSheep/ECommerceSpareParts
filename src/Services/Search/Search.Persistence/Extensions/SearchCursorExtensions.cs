using Lucene.Net.Search;
using Search.Abstractions.Models;

namespace Search.Persistence.Extensions;

public static class SearchCursorExtensions
{
    public static ScoreDoc ToScoreDoc(this SearchCursor cursor)
    {
        return new ScoreDoc(cursor.DocId, cursor.Score, cursor.ShardIndex);
    }

    public static SearchCursor ToCursor(this ScoreDoc scoreDoc)
    {
        return new SearchCursor
        {
            DocId = scoreDoc.Doc,
            Score = scoreDoc.Score,
            ShardIndex = scoreDoc.ShardIndex
        };
    }
}