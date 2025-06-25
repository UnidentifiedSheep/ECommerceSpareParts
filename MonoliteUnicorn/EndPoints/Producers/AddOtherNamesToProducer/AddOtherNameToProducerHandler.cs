using Core.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.AddOtherNamesToProducer;

public record AddOtherNameToProducerCommand(int ProducerId, string OtherName, string? WhereUsed) : ICommand<Unit>;

public class AddOtherNameToProducerValidation : AbstractValidator<AddOtherNameToProducerCommand>
{
    public AddOtherNameToProducerValidation()
    {
        RuleFor(x => x.OtherName)
            .NotEmpty()
            .WithMessage("Дополнительное имя не может быть пустым")
            .Must(name => name.Trim().Length >= 2)
            .WithMessage("Длина дополнительного имени должна быть не менее 2 символов")
            .Must(name => name.Trim().Length <= 64)
            .WithMessage("Длина дополнительного имени не может превышать 64 символов");

        RuleFor(x => x.WhereUsed)
            .Must(name => string.IsNullOrWhiteSpace(name) || name.Trim().Length <= 64)
            .WithMessage("Длина обозначения применения имени не может превышать 64 символов");
    }
}
public class AddOtherNameToProducerHandler(DContext context) : ICommandHandler<AddOtherNameToProducerCommand, Unit>
{
    public async Task<Unit> Handle(AddOtherNameToProducerCommand request, CancellationToken cancellationToken)
    {
        var producerFound = await context.Producers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProducerId, cancellationToken);
        if (!producerFound)
            throw new ProducerNotFoundException(request.ProducerId);
        
        var otherName = request.OtherName.Trim();
        var usage = request.WhereUsed?.Trim();
        
        var sameExists = await context.ProducersOtherNames
            .AsNoTracking()
            .AnyAsync(x => x.ProducerOtherName == otherName && 
                           x.ProducerId == request.ProducerId && 
                           x.WhereUsed == usage, cancellationToken);
        if (sameExists)
            throw new SameProducerOtherNameExistsException();
        await context.Database
            .ExecuteSqlAsync($"""
                              insert into producers_other_names (producer_id, producer_other_name, where_used) 
                              values ({request.ProducerId}, {otherName}, {usage});
                              """, cancellationToken);
        return Unit.Value;
    }
}