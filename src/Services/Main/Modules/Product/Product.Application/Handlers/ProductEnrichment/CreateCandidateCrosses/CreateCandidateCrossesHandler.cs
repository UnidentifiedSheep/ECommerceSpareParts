using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Entities.Product.Enums.Products;
using Main.Entities.Product.Exceptions;
using Main.Modules.Product.Application.Dtos.Product;
using Main.Modules.Product.Application.Products.Commands.MakeLinkageBetweenArticles;
using MediatR;

namespace Main.Modules.Product.Application.Handlers.ProductEnrichment.CreateCandidateCrosses;

[Transactional]
[AutoSave]
public record CreateCandidateCrossesCommand(
	Guid CandidateId,
	IReadOnlyCollection<Guid> CrossCandidateIds,
	ProductLinkageType LinkageType) : ICommand;

public class CreateCandidateCrossesHandler(ISender sender) : ICommandHandler<CreateCandidateCrossesCommand>
{
	public async Task<Unit> Handle(CreateCandidateCrossesCommand request, CancellationToken cancellationToken)
	{
		if (request.CrossCandidateIds.Contains(request.CandidateId))
			throw new ProductCrossSelfReferenceException();

		var productIds = await CreateProducts(request, cancellationToken);
		var leftProductId = productIds[request.CandidateId];

		await sender.Send(
			new MakeLinkageBetweenProductsCommand(
				request
					.CrossCandidateIds
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

	private async Task<Dictionary<Guid, int>>
		CreateProducts(CreateCandidateCrossesCommand request, CancellationToken ct)
		=> (await sender.Send(
			new AddCandidateToCatalogueCommand(
				request
					.CrossCandidateIds
					.Append(request.CandidateId)
					.Select(x => new AddCandidateToCatalogueItem(x, null))),
			ct)).CreatedIds;
}
