using Domain;

namespace Main.Entities;

//used for auditing
public class Event : AuditableEntity<Event, int> //TODO: create configuration file for schema
{
    public int Id { get; set; }

    public string AggregateId { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string JsonValue { get; set; } = null!;
    
    public override int GetId() => Id;
}