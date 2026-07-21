using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.User;

public class UserVehicle : AuditableEntity<UserVehicle, Guid>, ILinqEntity<UserVehicle, Guid>
{
    public const int MaxPlateNumberLength = 32;
    public const int MaxVinLength = 50;
    public const int MaxCommentLength = 500;

    private UserVehicle() { }

    private UserVehicle(
        Guid userId,
        Guid vehicleId,
        string plateNumber,
        string? vin,
        string? comment)
    {
        UserId = userId;
        SetVehicle(vehicleId);
        SetPlateNumber(plateNumber);
        SetVin(vin);
        SetComment(comment);
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid VehicleId { get; private set; }

    public string? Vin { get; private set; }

    public string PlateNumber { get; private set; } = null!;

    public string? Comment { get; private set; }

    public User User { get; private set; } = null!;

    public static Expression<Func<UserVehicle, Guid>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<UserVehicle, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    internal static UserVehicle Create(
        Guid userId,
        Guid vehicleId,
        string plateNumber,
        string? vin = null,
        string? comment = null)
    {
        return new UserVehicle(
            userId,
            vehicleId,
            plateNumber,
            vin,
            comment);
    }

    public void SetVehicle(Guid vehicleId)
    {
        VehicleId = vehicleId.EnsureNotEqual(Guid.Empty, "user.vehicle.id.not.empty");
    }

    public void SetPlateNumber(string plateNumber) { PlateNumber = NormalizePlateNumber(plateNumber); }

    public void SetVin(string? vin) { Vin = NormalizeVin(vin); }

    public void SetComment(string? comment)
    {
        Comment = comment
            .NullIfWhiteSpace()
            ?
            .EnsureMaxLength(MaxCommentLength, "user.vehicle.comment.max.length");
    }

    public static string NormalizePlateNumber(string plateNumber)
    {
        return plateNumber
            .TrimSafe()
            .EnsureNotNullOrWhiteSpace("user.vehicle.plate.number.not.empty")
            .EnsureMaxLength(MaxPlateNumberLength, "user.vehicle.plate.number.max.length")
            .ToUpperInvariant();
    }

    public static string? NormalizeVin(string? vin)
    {
        return vin
            .NullIfWhiteSpace()
            ?
            .EnsureMaxLength(MaxVinLength, "user.vehicle.vin.code.max.length")
            .ToUpperInvariant();
    }

    public override Guid GetId() { return Id; }
}