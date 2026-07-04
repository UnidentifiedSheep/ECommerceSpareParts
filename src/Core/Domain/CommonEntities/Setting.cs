using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json;
using Domain.Interfaces;

namespace Domain.CommonEntities;

public class Setting : AuditableEntity<Setting, string>, ILinqEntity<Setting, string>
{
    private Setting() { }

    protected Setting(string json) { Json = json; }

    public string Key { get; protected set; } = null!;

    public string Json { get; protected set; } = null!;

    public static Expression<Func<Setting, string>> GetKeySelector() { return x => x.Key; }

    public static Expression<Func<Setting, bool>> GetEqualityExpression(string key)
    {
        return x => x.Key == key;
    }

    public void SetData(string json) { Json = json; }

    public override string GetId() { return Key; }
}

public abstract class Setting<T> : Setting
{
    private T? _data;

    protected Setting(string key, string json) : base(json) { Key = key; }

    protected Setting(string key, T data) : base(Serialize(data))
    {
        Key = key;
        _data = data;
    }

    [NotMapped]
    public T Data => _data ??= Deserialize(Json);

    public void SetData(T data)
    {
        Json = Serialize(data);
        _data = data;
    }

    private static string Serialize(T data) { return JsonSerializer.Serialize(data); }

    private static T Deserialize(string json) { return JsonSerializer.Deserialize<T>(json)!; }
}