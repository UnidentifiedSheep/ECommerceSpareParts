using FluentValidation;

namespace Application.Handlers.Users.AddVehicleToGarage;

public class AddVehicleToGarageValidation : AbstractValidator<AddVehicleToGarageCommand>
{
    public AddVehicleToGarageValidation()
    {
        RuleFor(x => x.Vehicle.PlateNumber)
            .NotEmpty().WithMessage("Номер автомобиля не может пустым.");
        
        RuleFor(x => x.Vehicle.Manufacture)
            .NotEmpty().WithMessage("Марка автомобиля не может быть пустым.")
            .MaximumLength(50).WithMessage("Длина названия производителя должна быть не больше 50 символов.");
        
        RuleFor(x => x.Vehicle.Model)
            .NotEmpty().WithMessage("Модель не может быть пуста.")
            .MaximumLength(125).WithMessage("Максимальная длина модели авто 125 символов.");
    }
}