using FluentValidation;

namespace Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;

public class AddContentLogisticsToPurchaseValidation : AbstractValidator<AddContentLogisticsToPurchaseCommand>
{
    public AddContentLogisticsToPurchaseValidation()
    {
        RuleForEach(x => x.Contents)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.AreaM3)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Площадь должна быть больше или равна 0");

                z.RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Цена должна быть больше или равна 0");
                
                z.RuleFor(x => x.WeightKg)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Вес должен быть больше или равна 0");
            });
    }
}