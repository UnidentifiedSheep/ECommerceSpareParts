using FluentValidation;

namespace Application.Handlers.Balance.GetTransactions;

public class GetTransactionsValidation : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsValidation()
    {
        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => x.ReceiverId != x.SenderId || string.IsNullOrWhiteSpace(x.ReceiverId))
            .WithMessage("Отправитель и получатель не могут быть одинаковы");

        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => !string.IsNullOrWhiteSpace(x.ReceiverId) || !string.IsNullOrWhiteSpace(x.SenderId))
            .WithMessage("Должен быть указан хотя бы один: отправитель или получатель");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeStart.Date <= x.RangeEnd.Date)
            .WithMessage("Дата начала диапазона не может быть позже даты конца");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeEnd.Date <= x.RangeStart.Date.AddMonths(5))
            .WithMessage("Максимальный диапазон выборки — 5 месяцев");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(x => x.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}