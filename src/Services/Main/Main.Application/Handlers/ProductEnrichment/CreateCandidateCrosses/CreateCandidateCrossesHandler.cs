using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.MakeLinkageBetweenArticles;
using Main.Entities.Exceptions;
using Main.Enums.Products;
using MediatR;

namespace Main.Application.Handlers.ProductEnrichment.CreateCandidateCrosses;

[Transactional, AutoSave]
public record CreateCandidateCrossesCommand(
	Guid LeftCandidateId,
	IReadOnlyCollection<Guid> RightCandidateIds,
	ProductLinkageType LinkageType) : ICommand;

public class CreateCandidateCrossesHandler(
	ISender sender) : ICommandHandler<CreateCandidateCrossesCommand>
{
	public async Task<Unit> Handle(
		CreateCandidateCrossesCommand request,
		CancellationToken cancellationToken)
	{
		if (request.RightCandidateIds.Contains(request.LeftCandidateId))
			throw new ProductCrossSelfReferenceException();

		var productIds = await CreateProducts(request, cancellationToken);
		var leftProductId = productIds[request.LeftCandidateId];

		await sender.Send(
			new MakeLinkageBetweenProductsCommand(
				request.RightCandidateIds
					.Select(x => new NewProductLinkageDto
					{
						CrossProductId = productIds[x],
						LinkageType = request.LinkageType,
						ProductId = leftProductId
					})
					.ToList()),
			cancellationToken);

		return Unit.Value;
	}

	private async Task<Dictionary<Guid, int>> CreateProducts(CreateCandidateCrossesCommand request, CancellationToken ct)
		=> (await sender.Send(
			new AddCandidateToCatalogueCommand(
				request
					.RightCandidateIds
					.Append(request.LeftCandidateId)
					.Select(x => new AddCandidateToCatalogueItem(x, null))),
			ct)).CreatedIds;
}
