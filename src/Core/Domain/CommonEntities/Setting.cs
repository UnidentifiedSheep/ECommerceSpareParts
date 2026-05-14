using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json;

namespace Domain.CommonEntities;

public class Setting : AuditableEntity<Setting, string>
{
    private Setting()
    {
    }

    protected Setting(string json)
    {
        Json = json;
    }

    public string Key { get; protected set; } = null!;

    public string Json { get; protected set; } = null!;

    public override string GetId()
    {
        return Key;
    }

    public override Expression<Func<Setting, bool>> GetEqualityExpression(string key)
        => x => x.Key == key;
}

public abstract class Setting<T> : Setting
{
    private T? _data;

    protected Setting(string key, string json) : base(json)
    {
        Key = key;
    }

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

    private static string Serialize(T data)
    {
        return JsonSerializer.Serialize(data);
    }

    private static T Deserialize(string json)
    {
        return JsonSerializer.Deserialize<T>(json)!;
    }
}