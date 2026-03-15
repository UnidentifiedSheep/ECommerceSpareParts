namespace Abstractions.Models.Repository;

public class QueryPresets
{
    public static readonly QueryOptions TrackForUpdate = new QueryOptions().WithTracking().WithForUpdate();
    public static readonly QueryOptions Track = new QueryOptions().WithTracking();
    public static readonly QueryOptions ForUpdate = new QueryOptions().WithForUpdate();
}