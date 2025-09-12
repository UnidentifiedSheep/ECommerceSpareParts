using Application.Interfaces;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleCharacteristics;
using MediatR;

namespace Application.Handlers.ArticleCharacteristics.DeleteCharacteristics;

public record DeleteCharacteristicsCommand(int Id) : ICommand; 

public class DeleteCharacteristicsHandler(IArticleCharacteristicsRepository repository, 
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCharacteristicsCommand>
{
    public async Task<Unit> Handle(DeleteCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetCharacteristic(request.Id, true, cancellationToken)
                     ?? throw new ArticleCharacteristicsNotFoundException(request.Id);
        unitOfWork.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}