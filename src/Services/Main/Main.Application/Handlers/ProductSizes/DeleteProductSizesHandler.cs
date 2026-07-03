using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductSizes.DeleteProductSizes;

[AutoSave]
[Transactional]
public record DeleteArticleSizesCommand(int ProductId) : ICommand;

public class DeleteProductSizesHandler(
    IRepository<ProductSize, int> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteArticleSizesCommand>
{
    public async Task<Unit> Handle(DeleteArticleSizesCommand request, CancellationToken cancellationToken)
    {
        var sizes = await repository.GetById(request.ProductId, cancellationToken)
                    ?? throw new ProductSizesNotFoundException(request.ProductId);

        unitOfWork.Remove(sizes);
        return Unit.Value;
    }
}