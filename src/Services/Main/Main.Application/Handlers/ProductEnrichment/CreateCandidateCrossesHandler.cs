using Main.Enums.Products;

namespace Main.Application.Handlers.ProductEnrichment;

public record CreateCandidateCrossesCommand( //should we do full mapping between existing product(candidate where productId != null) etc.
	Guid LeftCandidateId,
	IEnumerable<Guid> RightCandidateIds,
	ProductLinkageType LinkageType);

public class CreateCandidateCrossesHandler
{

}
