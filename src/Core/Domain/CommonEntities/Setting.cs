using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Domain.CommonEntities;

public class Setting : AuditableEntity<Setting, string>
{
    public string Key { get; protected set; } = null!;

    public string Json { get; protected set; } = null!;

    private Setting() {}

    protected Setting(string json)
    {
        Json = json;
    }

    public override string GetId() => Key;
}

public abstract class Setting<T> : Setting
{
    private T? _data;
    
    [NotMapped]
    public T Data => _data ??= Deserialize(Json);

    protected Setting(string key, string json) : base(json)
    {
        Key = key;
    }

    protected Setting(string key, T data) : base(Serialize(data))
    {
        Key = key;
        _data = data;
    }

    public void SetData(T data)
    {
        Json = Serialize(data);
        _data = data;
    }

    private static string Serialize(T data)
        => JsonSerializer.Serialize(data);

    private static T Deserialize(string json)
        => JsonSerializer.Deserialize<T>(json)!;
}