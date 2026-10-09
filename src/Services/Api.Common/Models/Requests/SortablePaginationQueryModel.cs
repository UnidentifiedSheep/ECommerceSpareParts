using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Api.Common.Models.Requests;

public record SortablePaginationQueryModel : PaginationQueryModel
{
	[FromQuery(Name = "sortBy")]
	public StringValues SortBy { get; init; }
}
