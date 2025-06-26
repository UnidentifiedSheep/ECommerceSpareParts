using Core.Interface;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Storages.EditStorage;

public record EditStorageCommand(string StorageName, PatchStorageDto EditStorage) : ICommand<Unit>;

public class EditStorageValidation : AbstractValidator<EditStorageCommand>
{
    public EditStorageValidation()
    {
        RuleFor(x => x.EditStorage.Description.Value)
            .Must(x => x?.Trim().Length <= 256)
            .When(x => x.EditStorage.Description.IsSet)
            .WithMessage("Максимальная длина описания 256 символов");

        RuleFor(x => x.EditStorage.Location.Value)
            .Must(x => x?.Trim().Length <= 256)
            .When(x => x.EditStorage.Location.IsSet)
            .WithMessage("Максимальная длина локации 256 символов");
    }
}

public class EditStorageHandler(DContext context) : ICommandHandler<EditStorageCommand, Unit>
{
    public async Task<Unit> Handle(EditStorageCommand request, CancellationToken cancellationToken)
    {
        var storage = await context.Storages.FirstOrDefaultAsync(x => x.Name == request.StorageName, cancellationToken)
            ?? throw new StorageNotFoundException(request.StorageName);
        request.EditStorage.Adapt(storage);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}