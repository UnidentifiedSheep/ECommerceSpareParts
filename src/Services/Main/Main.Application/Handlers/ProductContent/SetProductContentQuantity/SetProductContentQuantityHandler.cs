using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Exceptions.Articles;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ArticleContent.SetArticleContentCount;

[AutoSave]
[Transactional]
public record SetProductsContentCountCommand(int ParentProductId, int ChildProductId, int Quantity) : ICommand;

public class SetProductContentQuantityHandler(
    IRepository<ProductContent, (int, int)> contentRepository) 
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