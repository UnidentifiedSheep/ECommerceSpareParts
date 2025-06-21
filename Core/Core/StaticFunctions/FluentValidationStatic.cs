using Core.Json;
using FluentValidation;

namespace Core.StaticFunctions;

public static class FluentValidationStatic
{
    public static IRuleBuilderOptions<T, TProp?> NotEmptyIfSet<T, TProp>(
        this IRuleBuilder<T, TProp?> ruleBuilder,
        Func<T, PatchField<TProp>> selector,
        string? message = null)
    {
        return ruleBuilder
            .NotEmpty()
            .When(x => selector(x).IsSet)
            .WithMessage(message ?? "Поле не может быть пустым");
    }

    public static IRuleBuilderOptions<T, string?> MinLengthIfSet<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        Func<T, PatchField<string?>> selector,
        int min,
        string? message = null)
    {
        return ruleBuilder
            .MinimumLength(min)
            .When(x => selector(x).IsSet)
            .WithMessage(message ?? $"Минимальная длина {min} символов");
    }

    public static void MaxLengthIfSet<T>(this IRuleBuilder<T, string?> ruleBuilder,
        Func<T, PatchField<string?>> selector,
        int max,
        string? message = null)
    {
        ruleBuilder
            .MaximumLength(max)
            .When(x => selector(x).IsSet)
            .WithMessage(message ?? $"Максимальная длина {max} символов");
    }
}