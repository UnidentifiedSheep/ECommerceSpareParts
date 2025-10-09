namespace Core.Dtos.Member.Vehicles;

public class VehicleDto
{
    public string? Vin { get; set; }

    public string PlateNumber { get; set; } = null!;

    public string Manufacture { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string? Modification { get; set; }

    public string? EngineCode { get; set; }

    public int? ProductionYear { get; set; }

    public string? Comment { get; set; }
}