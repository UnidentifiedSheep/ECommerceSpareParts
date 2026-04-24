using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions.Products;
using MediatR;

namespace Main.Application.Handlers.ProductContent.SetProductContentQuantity;

[AutoSave]
[Transactional]
public record SetProductsContentCountCommand(int ParentProductId, int ChildProductId, int Quantity) : ICommand;

public class SetProductContentQuantityHandler(
    IRepository<Entities.Product.ProductContent, (int, int)> contentRepository) 
    : ICommandHandler<SetProductsContentCountCommand>
{
    public async Task<Unit> Handle(SetProductsContentCountCommand request, CancellationToken cancellationToken)
    {
        var content = await contentRepository
            .GetById((request.ParentProductId, request.ChildProductId), cancellationToken) 
                      ?? throw new ProductContentNotFoundException(request.ParentProductId, request.ChildProductId);
        content.SetQuantity(request.Quantity);
        
        return Unit.Value;
    }
}