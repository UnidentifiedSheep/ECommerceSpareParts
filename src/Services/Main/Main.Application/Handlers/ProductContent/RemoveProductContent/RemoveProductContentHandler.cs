using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions.Products;
using MediatR;

namespace Main.Application.Handlers.ProductContent.RemoveProductContent;

[AutoSave]
[Transactional]
public record RemoveProductContentCommand(int ParentProductId, int ChildProductId) : ICommand;

public class RemoveProductContentHandler(
    IRepository<Entities.Product.ProductContent, (int, int)> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveProductContentCommand>
{
    public async Task<Unit> Handle(RemoveProductContentCommand request, CancellationToken cancellationToken)
    {
        var content =
            await repository.GetById((request.ParentProductId, request.ChildProductId), cancellationToken)
            ?? throw new ProductContentNotFoundException(request.ParentProductId, request.ChildProductId);
        unitOfWork.Remove(content);
        return Unit.Value;
    }
}