using Microsoft.AspNetCore.Mvc;

namespace Api.Common.Models.Requests;

public record SortablePaginationQueryModel : PaginationQueryModel
{
    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }
}