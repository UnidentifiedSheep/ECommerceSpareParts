using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json;
using Domain;

namespace Main.Entities.Event;

public abstract class Event : AuditableEntity<Event, int>
{
    protected Event()
    {
    }

    protected Event(string json)
    {
        Json = json;
    }

    public int Id { get; protected set; }

    public string Discriminator { get; protected set; } = null!;

    public string Json { get; protected set; } = null!;

    public override int GetId()
    {
        return Id;
    }

    public override Expression<Func<Event, bool>> GetEqualityExpression(int key)
        => x => x.Id == key;
}

public abstract class Event<T> : Event
    where T : class
{
    private T? _data;

    protected Event(string json) : base(json)
    {
    }

    protected Event()
    {
    }

    protected Event(T data)
        : base(Serialize(data))
    {
        _data = data;
    }

    [NotMapped]
    public T Data
    {
        get => _data ??= Deserialize(Json);
        init => _data = value;
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