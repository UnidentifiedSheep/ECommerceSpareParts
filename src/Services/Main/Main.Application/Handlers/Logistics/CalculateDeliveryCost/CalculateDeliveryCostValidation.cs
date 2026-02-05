using Core.Interfaces;
using FluentValidation;
using Main.Application.Extensions;

namespace Main.Application.Handlers.Logistics.CalculateDeliveryCost;

public class CalculateDeliveryCostValidation : AbstractValidator<CalculateDeliveryCostQuery>
{
    public CalculateDeliveryCostValidation()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Список элементов не может быть пустым.");

        RuleForEach(x => x.Items)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Количество должно быть больше нуля.");
            });

    }
}