using System.Collections.Immutable;
using System.Net;
using Abstractions.Interfaces.Exceptions;
using Abstractions.Models.Validation;

namespace Exceptions.Base;

public class ValidationException : Exception, IStatusCode
{
    public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public ImmutableList<ValidationErrorModel> Errors { get; }
    public ValidationException(IEnumerable<ValidationErrorModel> errors) 
        : base("Не удалось валидировать данные")
    {
        Errors = errors.ToImmutableList();
    }

}