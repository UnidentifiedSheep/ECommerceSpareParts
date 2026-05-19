using Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.Models.Requests;

public record PaginationQueryModel
{
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 0;

    [FromQuery(Name = "size")]
    public int Size { get; init; } = 20;

    public static implicit operator Pagination(PaginationQueryModel queryModel)
    {
        return new Pagination(queryModel.Page, queryModel.Size);
    }
}