using System.Reflection;
using Abstractions.Interfaces.Services;
using Attributes;
using MediatR;

namespace Application.Common.Behaviors;

public class SaveChangesBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly AutoSaveAttribute? AutoSave = 
        typeof(TRequest).GetCustomAttribute<AutoSaveAttribute>(true);
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);
        
        if (AutoSave != null && !unitOfWork.Context.SuppressAutoSave)
            await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return response;
    }
}