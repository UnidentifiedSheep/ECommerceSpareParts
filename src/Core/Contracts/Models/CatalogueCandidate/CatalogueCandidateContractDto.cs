namespace Contracts.Models.CatalogueCandidate;

public record CatalogueCandidateContractDto
{
	public required Guid Id { get; init; }

	public required string Sku { get; init; }

	public required int ProducerId { get; init; }

	public required int? MappedProductId { get; init; }

	public required IReadOnlyList<string> Names { get; init; }
}
