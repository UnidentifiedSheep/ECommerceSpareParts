namespace Core.Models;

public class SearchLogModel
{
    public SearchLogModel(Guid userId, string searchPlace, object query)
    {
        if (string.IsNullOrWhiteSpace(searchPlace))
            throw new ArgumentNullException(nameof(searchPlace), "Search place cannot be null or whitespace.");
        UserId = userId;
        SearchPlace = searchPlace;
        Query = query ?? throw new ArgumentNullException(nameof(query), "Query cannot be null.");
        SearchDateTime = DateTime.Now;
    }

    public Guid UserId { get; set; }

    public string SearchPlace { get; set; }

    public object Query { get; set; }

    public DateTime SearchDateTime { get; set; }
}