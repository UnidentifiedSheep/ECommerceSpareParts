namespace Main.Application.Models.Product;

public sealed record ProductGroupAssignment(
	string NormalizedSku,
	int ProducerId,
	int ProductGroupId,
	string Sku,
	string Producer);
