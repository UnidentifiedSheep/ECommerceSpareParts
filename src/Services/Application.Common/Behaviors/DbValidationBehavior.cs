using Abstractions.Interfaces;
using Application.Common.Abstractions;
using BulkValidation.Core.Plan;
using MediatR;

namespace Application.Common.Behaviors;

public class DbValidationBehavior<TRequest, TResponse>(IDbValidator dbValidator, 
    AbstractDbValidation<TRequest>? validation = null)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (validation == null) return await next(cancellationToken);
        
        var plan = new ValidationPlan();

        validation.Build(plan, request);

        if (plan.Build().Count > 0)
            await dbValidator.Validate(plan, true, cancellationToken);
        
        
        return await next(cancellationToken);
    }
}