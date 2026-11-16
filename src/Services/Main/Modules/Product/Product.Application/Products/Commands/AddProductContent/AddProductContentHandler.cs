using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Entities.Product.Product;
using MediatR;

namespace Main.Modules.Product.Application.Products.Commands.AddProductContent;

[AutoSave]
[Transactional]
public record AddProductContentCommand(int ParentProductId, Dictionary<int, int> Contents) : ICommand;

public class AddProductContentHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddProductContentCommand>
{
	public async Task<Unit> Handle(AddProductContentCommand request, CancellationToken cancellationToken)
	{
		var contents = request
			.Contents
			.Select(x => ProductContent.Create(
				request.ParentProductId,
				x.Key,
				x.Value))
			.ToList();

		await unitOfWork.AddRangeAsync(contents, cancellationToken);
		return Unit.Value;
	}
}
