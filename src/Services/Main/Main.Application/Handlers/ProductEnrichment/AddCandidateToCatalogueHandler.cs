using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEnrichment;

[Transactional, AutoSave]
public record AddCandidateToCatalogueCommand : ICommand<AddCandidateToCatalogueResult>
{
	public IReadOnlyList<AddCandidateToCatalogueItem> Items { get; }
	public AddCandidateToCatalogueCommand(Guid id, string? selectedName)
	{
		Items = [new AddCandidateToCatalogueItem(id, selectedName)];
	}

	public AddCandidateToCatalogueCommand(IEnumerable<AddCandidateToCatalogueItem> items)
	{
		Items = items.Distinct().ToList();
	}
}

public record AddCandidateToCatalogueItem(Guid Id, string? SelectedName);

public record AddCandidateToCatalogueResult(Dictionary<Guid, int> CreatedIds);

public class AddCandidateToCatalogueHandler(
	IRepository<CatalogueCandidate, Guid> repository,
	IReadRepository<CatalogueCandidate, Guid> readRepository,
	IUnitOfWork unitOfWork) : ICommandHandler<AddCandidateToCatalogueCommand, AddCandidateToCatalogueResult>
{
	public async Task<AddCandidateToCatalogueResult> Handle(
		AddCandidateToCatalogueCommand request,
		CancellationToken cancellationToken)
	{
		var ids = request.Items.Select(x => x.Id).Distinct().ToList();

		if (ids.Count != request.Items.Count)
			throw new CatalogueCandidateDuplicateIdsException();

		var candidates = await repository
			.EnsureExistsAsync(
				ids: ids,
				errorFactory: _ => new CatalogueCandidateNotFoundException(),
				ct: cancellationToken);

		var names = (await readRepository
			.Query
			.Where(x => ids.Contains(x.Id))
			.SelectMany(x => x.SupplierProducts)
			.SelectMany(x => x.Names)
			.Select(x => new
			{
				Id = x.SupplierProduct.CatalogueCandidateId!.Value,
				x.Name
			})
			.ToListAsync(cancellationToken))
			.ToLookup(x => x.Id, x => x.Name);

		var toAdd = new Dictionary<Guid, Product>(request.Items.Count);
		var productIds = new Dictionary<Guid, int>();

		foreach (var item in request.Items)
		{
			var candidate = candidates[item.Id];
			if (candidate.ProductId != null)
			{
				productIds[item.Id] = candidate.ProductId.Value;
				continue;
			}

			if (!string.IsNullOrWhiteSpace(item.SelectedName))
				toAdd[item.Id] = candidate.CreateProduct(item.SelectedName);
			else
				toAdd[item.Id] = candidate.CreateProduct(names[item.Id].First());
		}

		await unitOfWork.AddRangeAsync(toAdd.Values, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		foreach (var added in toAdd)
		{
			productIds[added.Key] = added.Value.Id;
			candidates[added.Key].MapToProduct(added.Value.Id);
		}

		return new AddCandidateToCatalogueResult(productIds);
	}
}
