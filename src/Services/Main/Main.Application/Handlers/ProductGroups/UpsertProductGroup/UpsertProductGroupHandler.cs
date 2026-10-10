using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Entities.Exceptions;
using Main.Entities.Product;

namespace Main.Application.Handlers.ProductGroups.UpsertProductGroup;

[AutoSave]
[Transactional]
public record UpsertProductGroupCommand(int? Id, string Name) : ICommand<UpsertProductGroupResult>;

public record UpsertProductGroupResult(ProductGroupDto Group);

public class UpsertProductGroupHandler(
	IUnitOfWork unitOfWork,
	IRepository<ProductGroup, int> repository,
	IProjectionProvider<ProductGroup, ProductGroupDto> projection)
	: ICommandHandler<UpsertProductGroupCommand, UpsertProductGroupResult>
{
	public async Task<UpsertProductGroupResult> Handle(
		UpsertProductGroupCommand request,
		CancellationToken cancellationToken)
	{
		var normalizedName = ProductGroup.NormalizeName(request.Name.Trim());
		var groups = await repository.ListAsync(
			Criteria<ProductGroup>.New()
				.Where(group => group.NormalizedName == normalizedName || group.Id == request.Id)
				.Track()
				.Build(),
			cancellationToken);

		var group = request.Id != null
			? groups.FirstOrDefault(item => item.Id == request.Id) ??
				throw new ProductGroupNotFoundException(request.Id.Value)
			: null;

		if (groups.Any(item => item.NormalizedName == normalizedName && item.Id != request.Id))
			throw new ProductGroupNameAlreadyExistsException();

		if (group is null)
		{
			group = ProductGroup.Create(request.Name);
			await unitOfWork.AddAsync(group, cancellationToken);
		}
		else
			group.SetName(request.Name);

		await unitOfWork.SaveChangesAsync(cancellationToken);
		return new UpsertProductGroupResult(projection.ProjectionFunc(group));
	}
}
