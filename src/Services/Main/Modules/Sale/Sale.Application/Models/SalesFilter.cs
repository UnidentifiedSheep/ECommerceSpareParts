using Application.Common.Models;
using Application.Common.Querying;
using Main.Entities.Sale.Enums;

namespace Main.Modules.Sale.Application.Models;

public sealed record SalesFilter
{
	public RangeModel<DateTime> DateRange { get; init; } = new();

	public IReadOnlyCollection<Guid> OrganizationIds { get; init; } = [];

	public IReadOnlyCollection<int> CurrencyIds { get; init; } = [];

	public IReadOnlyCollection<int> ProductIds { get; init; } = [];

	public IReadOnlyCollection<SaleState> States { get; init; } = [];

	public IReadOnlyCollection<SortExpression> SortBy { get; init; } = [];

	public string? SearchTerm { get; init; }
}
