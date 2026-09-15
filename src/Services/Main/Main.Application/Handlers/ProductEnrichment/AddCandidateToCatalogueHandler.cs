using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Product.Enrichment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEnrichment;

[Transactional, AutoSave]
public record AddCandidateToCatalogueCommand(
	Guid Id,
	string? SelectedName) : ICommand;

public class AddCandidateToCatalogueHandler(
	IRepository<CatalogueCandidate, Guid> repository,
	IReadRepository<CatalogueCandidate, Guid> readRepository,
	IUnitOfWork unitOfWork) : ICommandHandler<AddCandidateToCatalogueCommand>
{
	public async Task<Unit> Handle(
		AddCandidateToCatalogueCommand request,
		CancellationToken cancellationToken)
	{
		var candidate = await repository.GetById(request.Id, cancellationToken)
			?? throw new CatalogueCandidateNotFoundException();

		if (candidate.ProductId != null) return Unit.Value;

		Product product;

		if (!string.IsNullOrWhiteSpace(request.SelectedName))
			product = candidate.CreateProduct(request.SelectedName);
		else
		{
			var name = await readRepository
				.Query
				.Where(x => x.Id == request.Id)
				.SelectMany(x => x.SupplierProducts)
				.SelectMany(x => x.Names)
				.Select(x => x.Name)
				.FirstAsync(cancellationToken);

			product = candidate.CreateProduct(name);
		}

		await unitOfWork.AddAsync(product, cancellationToken);
		return Unit.Value;
	}
}
