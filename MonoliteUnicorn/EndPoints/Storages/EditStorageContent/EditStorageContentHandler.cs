using Core.Interface;
using Core.StaticFunctions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Exceptions;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Inventory;

namespace MonoliteUnicorn.EndPoints.Storages.EditStorageContent;

public record EditStorageContentCommand(Dictionary<int, (PatchStorageContentDto value, string concurrencyCode)> EditedFields, string UserId) : ICommand;

public class EditStorageContentValidation : AbstractValidator<EditStorageContentCommand>
{
    public EditStorageContentValidation()
    {
        RuleFor(x => x.EditedFields).NotEmpty()
            .WithMessage("Список отредактированных элементов не может быть пустым.");
        
        RuleFor(x => x.EditedFields)
            .Must(x => x.Count < 100)
            .WithMessage("Максимальное количество для редактирования за раз, не может превышать 100 элементов");
        
        RuleFor(x => x.EditedFields)
            .Custom((dict, context) =>
            {
                var backThreeMoth = DateTime.Now.AddMonths(-3);
                var hourAhead = DateTime.Now.AddHours(1);
                foreach (var (key, (value, _)) in dict)
                {
                    if (value.Count.IsSet && value.Count < 0)
                        context.AddFailure($"Элемент ID={key}", "Количество не может быть отрицательным");

                    if (value.BuyPrice.IsSet && Math.Round(value.BuyPrice, 2) <= 0)
                        context.AddFailure($"Элемент ID={key}", "Цена должна быть больше 0");
                    
                    if(value.PurchaseDatetime.IsSet && value.PurchaseDatetime < backThreeMoth || value.PurchaseDatetime > hourAhead)
                        context.AddFailure($"Элемент ID={key}", "Нельзя поменять дату которая отличается более чем на 3 месяца");
                }
            });
    }
}

public class EditStorageContentHandler(DContext context, IInventory inventory) : ICommandHandler<EditStorageContentCommand>
{
    public async Task<Unit> Handle(EditStorageContentCommand request, CancellationToken cancellationToken)
    {
        await using var dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var storageContentIds = request.EditedFields.Keys;
        var storageContents = 
            await context.EnsureStorageContentsExistForUpdate(storageContentIds, cancellationToken);

        foreach (var item in request.EditedFields)
        {
            var content = storageContents[item.Key];
            var currentConcurrencyCode = ConcurrencyStatic.GetConcurrencyCode(content.Id, content.ArticleId,
                content.BuyPrice, content.CurrencyId, content.StorageName, 
                content.BuyPriceInUsd, content.Count, content.PurchaseDatetime);
            var clientConcurrencyCode = item.Value.concurrencyCode;
            if (clientConcurrencyCode != currentConcurrencyCode) 
                throw new ConcurrencyCodeMismatchException(clientConcurrencyCode, currentConcurrencyCode);
        }
        
        var dict = request.EditedFields
            .ToDictionary(x => x.Key, x => x.Value.value);
        await inventory.EditStorageContent(dict, request.UserId, cancellationToken);
        
        await dbTransaction.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}