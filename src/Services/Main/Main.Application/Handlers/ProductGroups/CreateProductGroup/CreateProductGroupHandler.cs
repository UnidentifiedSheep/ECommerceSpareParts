using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Entities.Product;

namespace Main.Application.Handlers.ProductGroups.CreateProductGroup;

[AutoSave]
[Transactional]
public record CreateProductGroupCommand(string Name) : ICommand<CreateProductGroupResult>;

public record CreateProductGroupResult(ProductGroupDto Group);

public class CreateProductGroupHandler(
	IUnitOfWork unitOfWork,
	IProjectionProvider<ProductGroup, ProductGroupDto> projection)
	: ICommandHandler<CreateProductGroupCommand, CreateProductGroupResult>
{
	public async Task<CreateProductGroupResult> Handle(
		CreateProductGroupCommand request,
		CancellationToken cancellationToken)
	{
		var group = ProductGroup.Create(request.Name);
		await unitOfWork.AddAsync(group, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreateProductGroupResult(projection.ProjectionFunc(group));
	}
}
