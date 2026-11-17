using Application.Common.Extensions;
using Application.Common.Querying;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.Models.Requests;

public record SortablePaginationQueryModel : PaginationQueryModel
{
	[FromQuery(Name = "sortBy")]
	public string[] SortBy { get; init; } = [];

	public SortExpression[] ToSortExpressions() => SortBy.ToSortExpressions();
}
