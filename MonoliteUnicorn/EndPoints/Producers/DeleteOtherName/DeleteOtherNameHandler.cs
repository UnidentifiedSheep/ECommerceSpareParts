using Core.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Producers.DeleteOtherName;

public record DeleteOtherNameCommand(int ProducerId, string OtherName, string? Usage) : ICommand<Unit>;

public class DeleteOtherNameHandler(DContext context) : ICommandHandler<DeleteOtherNameCommand, Unit>
{
    public async Task<Unit> Handle(DeleteOtherNameCommand request, CancellationToken cancellationToken)
    {
        _ = await context.Producers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProducerId, cancellationToken) 
            ?? throw new ProducerNotFoundException(request.ProducerId);
        
        var exists = await context.ProducersOtherNames.AsNoTracking()
            .AnyAsync(x => x.ProducerOtherName == request.OtherName &&
                                      x.ProducerId == request.ProducerId && 
                                      x.WhereUsed == request.Usage, cancellationToken);
        
        if (!exists) throw new ProducersOtherNameNotFoundException(request.OtherName);

        await context.Database.ExecuteSqlAsync($"""
                                                delete from producers_other_names 
                                                       where producer_id = {request.ProducerId} and
                                                             producer_other_name = {request.OtherName} and
                                                             where_used = {request.Usage}
                                                """, cancellationToken);
        return Unit.Value;
    }
}