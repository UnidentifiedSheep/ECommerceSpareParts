using Core.Interface;
using Core.RabbitMq.Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Markups;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Markups.SelectDefaultMarkup;

public record SelectDefaultMarkupCommand(int MarkupGroupId) : ICommand;
public class SelectDefaultMarkupHandler(DContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<SelectDefaultMarkupCommand>
{
    public async Task<Unit> Handle(SelectDefaultMarkupCommand request, CancellationToken cancellationToken)
    {
        var selectedMarkup = await context.DefaultSettings
            .FirstAsync(x => x.Key == "SelectedMarkupId", cancellationToken);
        if(selectedMarkup.Value == request.MarkupGroupId.ToString())
            return Unit.Value;
        if (request.MarkupGroupId == -1)
            selectedMarkup.Value = "-1";
        else
        {
            var markupGroup = await context.MarkupGroups
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(x => x.Id == request.MarkupGroupId, cancellationToken) 
                              ?? throw new MarkupGroupNotFoundException(request.MarkupGroupId);
            selectedMarkup.Value = markupGroup.Id.ToString();
        }
        await context.SaveChangesAsync(cancellationToken);
        await publishEndpoint.Publish(new MarkupGroupChangedEvent(request.MarkupGroupId), cancellationToken);
        return Unit.Value;
    }
}