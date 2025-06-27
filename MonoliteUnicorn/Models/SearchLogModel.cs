using Newtonsoft.Json.Linq;

namespace MonoliteUnicorn.Models;

public class SearchLogModel
{
    public SearchLogModel(string userId, string searchPlace, object query)
    {
        if(string.IsNullOrWhiteSpace(searchPlace))
            throw new ArgumentNullException(nameof(searchPlace), "Search place cannot be null or whitespace.");
        if(query == null)
            throw new ArgumentNullException(nameof(query), "Query cannot be null.");
        UserId = userId;
        SearchPlace = searchPlace;
        Query = JObject.FromObject(query);
        SearchDateTime = DateTime.Now;
    }
    public string UserId { get; set; }

    public string SearchPlace { get; set; }

    public JObject Query { get; set; }

    public DateTime SearchDateTime { get; set; }
    
}