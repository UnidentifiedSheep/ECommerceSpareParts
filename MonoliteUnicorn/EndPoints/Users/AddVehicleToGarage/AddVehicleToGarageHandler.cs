using Core.Interface;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Member.Vehicles;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.Exceptions.Vehicles;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Users.AddVehicleToGarage;

public record AddVehicleToGarageCommand(VehicleDto Vehicle, string UserId) : ICommand<Unit>;

public class AddVehicleToGarageValidation : AbstractValidator<AddVehicleToGarageCommand>
{
    public AddVehicleToGarageValidation()
    {
        RuleFor(x => x.Vehicle.PlateNumber).NotEmpty().WithMessage("Номер автомобиля не может пустым.");
        RuleFor(x => x.Vehicle.Manufacture).NotEmpty().WithMessage("Марка автомобиля не может быть пустым.");
        RuleFor(x => x.Vehicle.Model).NotEmpty().WithMessage("Модель не может быть пуста.");
    }
}

public class AddVehicleToGarageHandler(DContext context) : ICommandHandler<AddVehicleToGarageCommand, Unit>
{
    public async Task<Unit> Handle(AddVehicleToGarageCommand request, CancellationToken cancellationToken)
    {
        var userExists = await context.AspNetUsers.AsNoTracking().AnyAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        if (!userExists) throw new UserNotFoundException(request.UserId);
        
        var vin = request.Vehicle.Vin?.Trim();
        var plateNumber = request.Vehicle.PlateNumber.Trim();
        
        var sameVinCodeTaken = await context.UserVehicles.AsNoTracking().AnyAsync(x => x.Vin == vin, cancellationToken: cancellationToken);
        if (sameVinCodeTaken) throw new VinCodeAlreadyTakenException(vin);
        
        var samePlateNumber = await context.UserVehicles.AsNoTracking().AnyAsync(x => x.PlateNumber == plateNumber, cancellationToken: cancellationToken);
        if (samePlateNumber) throw new PlateNumberAlreadyTakenException(plateNumber);
        
        var model = request.Vehicle.Adapt<UserVehicle>();
        model.UserId = request.UserId;
        await context.UserVehicles.AddAsync(model, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}