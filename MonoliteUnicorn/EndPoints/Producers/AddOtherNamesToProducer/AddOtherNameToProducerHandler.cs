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
        RuleFor(x => x.OtherName).NotEmpty().WithMessage("Дополнительные имена не могут быть пустыми");
        RuleFor(x => x.OtherName).MinimumLength(2).WithMessage("Длина дополнительного имени не может быть короче 2 символов");
        RuleFor(x => x.OtherName).MaximumLength(64).WithMessage("Длина дополнительного имени не может быть больше 64 символов");
        RuleFor(x => x.WhereUsed).MaximumLength(64).WithMessage("Длина обозначения применения имени не может быть больше 64 символов");
    }
}
public class AddOtherNameToProducerHandler(DContext context) : ICommandHandler<AddOtherNameToProducerCommand, Unit>
{
    public async Task<Unit> Handle(AddOtherNameToProducerCommand request, CancellationToken cancellationToken)
    {
        _ = await context.Producers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProducerId, cancellationToken) 
            ?? throw new ProducerNotFoundException(request.ProducerId);
        var otherName = request.OtherName.Trim();
        var usage = request.WhereUsed?.Trim();
        await context.Database
            .ExecuteSqlAsync($"""
                              insert into producers_other_names (producer_id, producer_other_name, where_used) 
                              values ({request.ProducerId}, {otherName}, {usage});
                              """, cancellationToken);
        return Unit.Value;
    }
}