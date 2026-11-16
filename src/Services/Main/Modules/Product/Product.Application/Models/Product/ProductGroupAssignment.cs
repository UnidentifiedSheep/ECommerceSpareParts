namespace Main.Modules.Product.Application.Models.Product;

public sealed record ProductGroupAssignment(
	string NormalizedSku,
	int ProducerId,
	int ProductGroupId);
