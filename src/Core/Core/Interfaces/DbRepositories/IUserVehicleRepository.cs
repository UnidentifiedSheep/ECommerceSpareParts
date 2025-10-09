namespace Core.Interfaces.DbRepositories;

public interface IUserVehicleRepository
{
    Task<bool> VehicleVinCodeTaken(string vinCode, CancellationToken cancellationToken = default);
    Task<bool> VehiclePlateNumberTaken(string plateNumber, CancellationToken cancellationToken = default);
}