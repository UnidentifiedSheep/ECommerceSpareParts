using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities.Purchase;

namespace Main.Application.Handlers.Purchases.EditPurchase;

[Transactional]
[AutoSave]
public record EditPurchaseCommand(
    IEnumerable<EditPurchaseDto> Content,
    Guid PurchaseId,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime) : ICommand<EditPurchaseResult>;

/// <param name="EditedCounts">
///     Словарь где Key - айди артикула,
///     далее словарь где Key - цена и Value - количество.
///     Если количество отрицательное, то это количество, которое взяли со склада.
///     Если положительное, то вернули на склад.
/// </param>
public record EditPurchaseResult(Dictionary<int, Dictionary<decimal, int>> EditedCounts);

public class EditPurchaseHandler(
    IRepository<Purchase, Guid> purchaseRepository,
    IRepository<PurchaseContent, int> contentRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<EditPurchaseCommand, EditPurchaseResult>
{
    public async Task<EditPurchaseResult> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}