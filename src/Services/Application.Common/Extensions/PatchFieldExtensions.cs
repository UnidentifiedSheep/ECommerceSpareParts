using Abstractions.Models;

namespace Application.Common.Extensions;

public static class PatchFieldExtensions
{
    public static void Apply<T>(this PatchField<T> field, Action<T> apply)
    {
        if (field.IsSet)
            apply(field.Value!);
    }
}