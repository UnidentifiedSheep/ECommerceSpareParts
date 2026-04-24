using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Amw.ArticleCharacteristics;
using Main.Entities.Exceptions.Products;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductCharacteristics.PatchCharacteristics;

[AutoSave]
[Transactional]
public record PatchCharacteristicsCommand(int ProductId, string Name, PatchCharacteristicsDto Patch) : ICommand;

public class PatchCharacteristicsHandler(
    IRepository<ProductCharacteristic, (int, string)> repository
    ) : ICommandHandler<PatchCharacteristicsCommand>
{
    public async Task<Unit> Handle(PatchCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById((request.ProductId, request.Name), cancellationToken)
                     ?? throw new ProductCharacteristicsNotFoundException(request.ProductId, request.Name);
        
        request.Patch.Value.Apply(entity.SetValue);
        return Unit.Value;
    }
}