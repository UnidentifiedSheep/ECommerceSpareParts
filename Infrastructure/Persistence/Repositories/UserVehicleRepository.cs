using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class UserVehicleRepository(DContext context) : IUserVehicleRepository
{
    public async Task<bool> VehicleVinCodeTaken(string vinCode, CancellationToken cancellationToken = default)
    {
        return await context.UserVehicles.AsNoTracking().AnyAsync(x => x.Vin == vinCode.Trim(), cancellationToken);
    }

    public async Task<bool> VehiclePlateNumberTaken(string plateNumber, CancellationToken cancellationToken = default)
    {
        return await context.UserVehicles.AsNoTracking()
            .AnyAsync(x => x.PlateNumber == plateNumber.Trim(), cancellationToken);
    }
}