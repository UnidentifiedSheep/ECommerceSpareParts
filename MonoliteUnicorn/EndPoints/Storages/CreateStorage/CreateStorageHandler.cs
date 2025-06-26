using Core.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Storages.CreateStorage;

public record CreateStorageCommand(string Name, string? Description, string? Location) : ICommand<Unit>;

public class CreateStorageValidation : AbstractValidator<CreateStorageCommand>
{
    public CreateStorageValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Название не может быть пустым")
            .MinimumLength(6)
            .WithMessage("Минимальная длина названия 6 символов")
            .MaximumLength(128)
            .WithMessage("Максимальная длина названия 128 символов");
        RuleFor(x => x.Description)
            .MaximumLength(256)
            .WithMessage("Максимальная длина описания 256 символов");
        RuleFor(x => x.Location)
            .MaximumLength(256)
            .WithMessage("Максимальная длина локации 256 символов");
    }
}

public class CreateStorageHandler(DContext context) : ICommandHandler<CreateStorageCommand, Unit>
{
    public async Task<Unit> Handle(CreateStorageCommand request, CancellationToken cancellationToken)
    {
        var trimmedName = request.Name.Trim();
        var foundStorage = await context.Storages
            .AsNoTracking()
            .AnyAsync(x => x.Name == trimmedName, cancellationToken);
        if (foundStorage) 
            throw new StorageNameIsTakenException(trimmedName);
        var model = new Storage
        {
            Name = trimmedName,
            Description = request.Description?.Trim(),
            Location = request.Location?.Trim(),
        };
        await context.Storages.AddAsync(model, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}