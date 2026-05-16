using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.User;

public class UserVehicle : AuditableEntity<UserVehicle, Guid>, ILinqEntity<UserVehicle, Guid>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Vin { get; set; }

    public string PlateNumber { get; set; } = null!;

    public string Manufacture { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string? Modification { get; set; }

    public string? EngineCode { get; set; }

    public int? ProductionYear { get; set; }

    public string? Comment { get; set; }

    public static Expression<Func<UserVehicle, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public override Guid GetId()
    {
        return Id;
    }
}