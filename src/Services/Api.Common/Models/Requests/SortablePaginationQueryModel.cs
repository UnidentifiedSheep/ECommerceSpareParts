using Microsoft.AspNetCore.Mvc;
using Application.Common.Querying;
using Application.Common.Extensions;

namespace Api.Common.Models.Requests;

public record SortablePaginationQueryModel : PaginationQueryModel
{
	[FromQuery(Name = "sortBy")]
	public string[] SortBy { get; init; } = [];

	public SortExpression[] ToSortExpressions() => SortBy.ToSortExpressions();
}
