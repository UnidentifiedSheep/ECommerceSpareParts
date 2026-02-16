namespace Search.Api.EndPoints;

public static class SuggestionEndPoints
{
    public static IEndpointRouteBuilder MapSuggestionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/suggestions");

        group.MapGet("/", GetAll);

        return routes;
    }

    static IResult GetAll(string suggestion)
    {
        throw new NotImplementedException();
    }
}