using System.Collections.Immutable;
using Abstractions.Models.Validation;

namespace Exceptions.Base;

public class ValidationException : Exception
{
    public ImmutableList<ValidationErrorModel> Errors { get; }
    public ValidationException(IEnumerable<ValidationErrorModel> errors) 
        : base("Не удалось валидировать данные")
    {
        Errors = errors.ToImmutableList();
    }
}