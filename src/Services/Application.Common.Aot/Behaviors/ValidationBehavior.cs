using Abstractions.Models.Validation;
using Application.Common.Aot.Interfaces;
using Exceptions.Base;
using Mediator;
using Sannr;

namespace Application.Common.Aot.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IValidation<TRequest>? validation = null)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        if (validation == null) return await next(message, cancellationToken);
        
        ValidationResult result = await validation.ValidateAsync(message);
        
        if (result.IsValid) return await next(message, cancellationToken);
        
        var errors = result.Errors
            .Select(x => new ValidationErrorModel(x.MemberName, x.Message, null));
        throw new ValidationException(errors);

    }
}