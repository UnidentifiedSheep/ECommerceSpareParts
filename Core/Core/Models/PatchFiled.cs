namespace Core.Models;

public class PatchField<T>
{
    public bool IsSet { get; set; }
    public T? Value { get; set; }

    public static PatchField<T> NotSet()
    {
        return new PatchField<T> { IsSet = false };
    }

    public static PatchField<T> From(T? value)
    {
        return new PatchField<T> { IsSet = true, Value = value };
    }

    public static implicit operator T?(PatchField<T> patchField)
    {
        return patchField.Value;
    }
}