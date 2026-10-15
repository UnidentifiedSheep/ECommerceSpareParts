using Search.Enums;
namespace Search.Persistence.Models;

internal sealed record ProductDocumentSeed(int Id)
{
	public ProductReadinessStatus ReadinessStatus => ProductReadinessStatus.Partial;
}
