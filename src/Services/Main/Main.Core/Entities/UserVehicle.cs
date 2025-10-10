namespace Main.Core.Entities;

public class UserVehicle
{
    public string Id { get; set; } = null!;

    public Guid UserId { get; set; }

    public string? Vin { get; set; }

    public string PlateNumber { get; set; } = null!;

    public string Manufacture { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string? Modification { get; set; }

    public string? EngineCode { get; set; }

    public int? ProductionYear { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}