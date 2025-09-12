using FluentValidation;
using MediatR;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validationRules)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var validationResult =
            await Task.WhenAll(validationRules.Select(x => x.ValidateAsync(context, cancellationToken)));

        var errors = validationResult.Where(x => x.Errors.Any()).SelectMany(x => x.Errors).ToList();
        if (errors.Count != 0)
            throw new ValidationException(errors);

        return await next(cancellationToken);
    }
}