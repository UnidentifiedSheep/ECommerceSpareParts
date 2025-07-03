using Core.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.Markups;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Markups.DeleteMarkupGroup;

public record DeleteMarkupGroupCommand(int Id) : ICommand;
public class DeleteMarkupGroupHandler(DContext context) : ICommandHandler<DeleteMarkupGroupCommand>
{
    public async Task<Unit> Handle(DeleteMarkupGroupCommand request, CancellationToken cancellationToken)
    {
        var markupGroup = await context.MarkupGroups
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new MarkupGroupNotFoundException(request.Id);
        var currentSettings = await context.DefaultSettings
            .AsNoTracking()
            .FirstAsync(x => x.Key == "SelectedMarkupId", cancellationToken);
        var currentGroupId = int.Parse(currentSettings.Value);
        if(currentGroupId == request.Id)
            throw new MarkupGroupCanNotBeDeletedException();
        context.MarkupGroups.Remove(markupGroup);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}