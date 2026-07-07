namespace Contracts.Models.Supplier;

public class ContractSupplierProductDto
{
    public required string Id { get; init; }
    public required string Number { get; init; }
    public required string Brand { get; init; }
    public required string Name { get; init; }

    public required IReadOnlyList<ContractSupplierProductDto> Analogues { get; init; } = [];
    public required IReadOnlyList<ContractSupplierPositionDto> Positions { get; init; } = [];
}