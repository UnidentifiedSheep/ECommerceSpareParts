using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Domain;

namespace Main.Entities.Event;

public abstract class Event : AuditableEntity<Event, int>
{
    public int Id { get; protected set; }

    public string Discriminator { get; protected set; } = null!;
    
    public string Json { get; protected set; } = null!;
    
    private Event() {}

    protected Event(string json)
    {
        Json = json;
    }
    
    public override int GetId() => Id;
}

public abstract class Event<T> : Event
    where T : class
{
    private T? _data;

    [NotMapped]
    public T Data
    {
        get => _data ??= Deserialize(Json);
        init => _data = value;
    }

    protected Event(string json) : base(json) { }

    protected Event(T data)
        : base(Serialize(data))
    {
        _data = data;
    }

    private static string Serialize(T data)
        => JsonSerializer.Serialize(data);

    private static T Deserialize(string json)
        => JsonSerializer.Deserialize<T>(json)!;
}