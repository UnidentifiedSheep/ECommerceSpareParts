using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Vehicles;
using Main.Abstractions.Dtos.Member.Vehicles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Users.AddVehicleToGarage;

[Transactional]
public record AddVehicleToGarageCommand(VehicleDto Vehicle, Guid UserId) : ICommand<Unit>;

public class AddVehicleToGarageHandler(IUserVehicleRepository vehicleRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<AddVehicleToGarageCommand, Unit>
{
    public async Task<Unit> Handle(AddVehicleToGarageCommand request, CancellationToken cancellationToken)
    {
        var vin = request.Vehicle.Vin?.Trim();
        var plateNumber = request.Vehicle.PlateNumber.Trim();

        await ValidateData(vin, plateNumber, cancellationToken);

        var model = request.Vehicle.Adapt<UserVehicle>();
        model.UserId = request.UserId;
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ValidateData(string? vin, string plateNumber, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(vin) && await vehicleRepository.VehicleVinCodeTaken(vin, cancellationToken))
            throw new VinCodeAlreadyTakenException(vin);
        if (await vehicleRepository.VehiclePlateNumberTaken(plateNumber, cancellationToken))
            throw new PlateNumberAlreadyTakenException(plateNumber);
    }
}