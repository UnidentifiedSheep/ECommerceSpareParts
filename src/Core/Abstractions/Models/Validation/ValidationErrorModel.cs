namespace Abstractions.Models.Validation;

public sealed record ValidationErrorModel(string PropertyName, string ErrorMessage, object? AttemptedValue);