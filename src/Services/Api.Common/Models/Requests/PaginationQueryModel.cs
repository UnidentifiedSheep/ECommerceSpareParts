using Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.Models.Requests;

public record PaginationQueryModel
{
    [FromQuery(Name = "page")]
    public int? Page { get; init; }

    [FromQuery(Name = "size")]
    public int? Size { get; init; }

    public static implicit operator Pagination(PaginationQueryModel queryModel)
    {
        return new Pagination(queryModel.Page ?? 0, queryModel.Size ?? 20);
    }
}