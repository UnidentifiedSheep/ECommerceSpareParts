using Application.Common.Interfaces;
using Core.Dtos.Member.Vehicles;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Vehicles;
using Main.Application.Extensions;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Users.AddVehicleToGarage;

public record AddVehicleToGarageCommand(VehicleDto Vehicle, Guid UserId) : ICommand<Unit>;

public class AddVehicleToGarageHandler(
    IUserVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IUserRepository usersRepository) : ICommandHandler<AddVehicleToGarageCommand, Unit>
{
    public async Task<Unit> Handle(AddVehicleToGarageCommand request, CancellationToken cancellationToken)
    {
        var vin = request.Vehicle.Vin?.Trim();
        var plateNumber = request.Vehicle.PlateNumber.Trim();
        var userId = request.UserId;

        await ValidateData(vin, plateNumber, userId, cancellationToken);

        var model = request.Vehicle.Adapt<UserVehicle>();
        model.UserId = userId;
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(string? vin, string plateNumber, Guid userId,
        CancellationToken cancellationToken = default)
    {
        await usersRepository.EnsureUsersExists([userId], cancellationToken);
        if (!string.IsNullOrWhiteSpace(vin) && await vehicleRepository.VehicleVinCodeTaken(vin, cancellationToken))
            throw new VinCodeAlreadyTakenException(vin);
        if (await vehicleRepository.VehiclePlateNumberTaken(plateNumber, cancellationToken))
            throw new PlateNumberAlreadyTakenException(plateNumber);
    }
}