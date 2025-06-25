using Core.Interface;
using FluentValidation;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.CreateProducer;

public record CreateProducerCommand(AmwNewProducerDto NewProducer) : ICommand<CreateProducerResult>;
public record CreateProducerResult(int ProducerId);
public class CreateProducerValidation : AbstractValidator<CreateProducerCommand>
{
    public CreateProducerValidation()
    {
        RuleFor(x => x.NewProducer.ProducerName)
            .NotEmpty()
            .WithMessage("Название производителя не может быть пустым")
            .Must(name => name?.Trim().Length >= 2)
            .WithMessage("Минимальная длина названия производителя — 2 символа")
            .Must(name => name?.Trim().Length <= 64)
            .WithMessage("Максимальная длина названия производителя — 64 символа");

        RuleFor(x => x.NewProducer.Description)
            .Must(desc => desc?.Trim().Length <= 500)
            .When(x => !string.IsNullOrWhiteSpace(x.NewProducer.Description))
            .WithMessage("Максимальная длина описания — 500 символов");
    }
}

public class CreateProducerHandler(DContext context) : ICommandHandler<CreateProducerCommand, CreateProducerResult>
{
    public async Task<CreateProducerResult> Handle(CreateProducerCommand request, CancellationToken cancellationToken)
    {
        await using var dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var newProducers = request.NewProducer.Adapt<Producer>();
        await context.Producers.AddAsync(newProducers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);
        return new CreateProducerResult(newProducers.Id);
    }
}