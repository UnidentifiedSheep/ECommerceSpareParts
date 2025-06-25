using Core.Interface;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.EditProducer;

public record EditProducerCommand(int ProducerId, PatchProducerDto EditProducer) : ICommand<Unit>;

public class EditProducerValidation : AbstractValidator<EditProducerCommand>
{
    public EditProducerValidation()
    {
        RuleFor(x => x.EditProducer.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .When(x => x.EditProducer.Name.IsSet)
            .WithMessage("Название производителя не может быть пустым")
            .Must(name => name.Value?.Trim().Length >= 2)
            .When(x => x.EditProducer.Name.IsSet)
            .WithMessage("Минимальная длина названия производителя — 2 символа")
            .Must(name => name.Value?.Trim().Length <= 64)
            .When(x => x.EditProducer.Name.IsSet)
            .WithMessage("Максимальная длина названия производителя — 64 символа");
        
        RuleFor(x => x.EditProducer.Description)
            .Must(desc => desc.Value?.Trim().Length <= 500)
            .When(x => x.EditProducer.Description.IsSet)
            .WithMessage("Максимальная длина описания — 500 символов");
    }
}

public class EditProducerHandler(DContext context) : ICommandHandler<EditProducerCommand, Unit>
{
    public async Task<Unit> Handle(EditProducerCommand request, CancellationToken cancellationToken)
    {
        var producer = await context.Producers
            .FirstOrDefaultAsync(x => x.Id == request.ProducerId, cancellationToken) ?? throw new ProducerNotFoundException(request.ProducerId);
        request.EditProducer.Adapt(producer);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}