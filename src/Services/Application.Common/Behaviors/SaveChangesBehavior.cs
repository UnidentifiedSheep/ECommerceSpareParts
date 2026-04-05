using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

public class SaveChangesBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);
        
        if (request is IAutoSaveCommand && !unitOfWork.Context.SuppressAutoSave)
            await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return response;
    }
}