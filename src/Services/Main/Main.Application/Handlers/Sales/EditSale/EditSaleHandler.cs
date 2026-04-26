using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Dtos.Amw.Sales;
using Main.Application.Models.SaleService;
using Main.Entities.Exceptions.Sales;
using Main.Entities.Sale;
using MediatR;

namespace Main.Application.Handlers.Sales.EditSale;

[AutoSave]
[Transactional]
public record EditSaleCommand(
    IEnumerable<EditSaleContentDto> EditedContent,
    IEnumerable<StorageLot> StorageContentValues,
    Guid SaleId,
    int CurrencyId,
    DateTime SaleDateTime,
    string? Comment) : ICommand;

public class EditSaleHandler(
    IUnitOfWork unitOfWork, 
    ISaleService saleService, 
    IRepository<Sale, Guid> saleRepository,
    IRepository<SaleContent, int> saleContentRepository)
    : ICommandHandler<EditSaleCommand>
{
    public async Task<Unit> Handle(EditSaleCommand request, CancellationToken cancellationToken)
    {
        var saleId = request.SaleId;
        var editedContent = request.EditedContent.ToList();

        var sale = await saleRepository.GetById(saleId, cancellationToken)
                   ?? throw new SaleNotFoundException(saleId);

        sale.SetComment(request.Comment);
        sale.SetDateTime(request.SaleDateTime);
        sale.SetCurrency(request.CurrencyId);
        
        var saleContents = await GetContents(saleId, cancellationToken);

        var deletedContentIds = new HashSet<int>(saleContents.Keys);

        foreach (var item in editedContent)
        {
            if (item.Id.HasValue) deletedContentIds.Remove(item.Id.Value);
            
        }
        
        foreach (var saleContent in saleService.DistributeDetails(request.StorageContentValues, editedContent))
            sale.AddContent(saleContent);

        var deletedContents = saleContents
            .Where(kvp => deletedContentIds.Contains(kvp.Key))
            .Select(x => x.Value)
            .ToList();
        
        unitOfWork.RemoveRange(deletedContents);
        return Unit.Value;
    }

    private async Task<Dictionary<int, SaleContent>> GetContents(Guid saleId, CancellationToken cancellationToken)
    {
        var criteria = Criteria<SaleContent>.New()
            .Where(x => x.SaleId == saleId)
            .Include(x => x.Details)
            .Track()
            .Build();
        
        return (await saleContentRepository.ListAsync(criteria, cancellationToken)).ToDictionary(x => x.Id);
    }
}