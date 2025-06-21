using Mapster;

namespace Core.Json;

public class PatchField<T>
{
    public bool IsSet { get; set; }
    public T? Value { get; set; }

    public static PatchField<T> NotSet() => new() { IsSet = false };
    public static PatchField<T> From(T? value) => new() { IsSet = true, Value = value };
    
    public static implicit operator T?(PatchField<T> patchField) => patchField.Value;
}
