using FluentValidation;
using Localization.Domain.Extensions;

namespace Main.Application.Handlers.Users.AddVehicleToGarage;

public class AddVehicleToGarageValidation : AbstractValidator<AddVehicleToGarageCommand>
{
    public AddVehicleToGarageValidation()
    {
        RuleFor(x => x.Vehicle.PlateNumber)
            .NotEmpty()
            .WithLocalizationKey("user.vehicle.plate.number.not.empty");

        RuleFor(x => x.Vehicle.Manufacture)
            .NotEmpty()
            .WithLocalizationKey("user.vehicle.manufacture.not.empty")
            .MaximumLength(50)
            .WithLocalizationKey("user.vehicle.manufacture.max.length");

        RuleFor(x => x.Vehicle.Model)
            .NotEmpty()
            .WithLocalizationKey("user.vehicle.model.not.empty")
            .MaximumLength(125)
            .WithLocalizationKey("user.vehicle.model.max.length");
    }
}