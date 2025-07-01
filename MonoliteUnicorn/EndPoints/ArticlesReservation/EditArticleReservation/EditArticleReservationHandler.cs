using Core.Interface;
using FluentValidation;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;
using MonoliteUnicorn.Services.ArticleReservations;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.EditArticleReservation;

public record EditArticleReservationCommand(int ReservationId, EditArticleReservationDto NewValue, string WhoUpdated) : ICommand;

public class EditArticleReservationValidation : AbstractValidator<EditArticleReservationCommand>
{
    public EditArticleReservationValidation()
    {
        RuleFor(z => z.NewValue.GivenPrice)
            .Must(z => Math.Round(z!.Value, 2) > 0)
            .When(z => z.NewValue.GivenPrice != null)
            .WithMessage("Предложенная цена должна быть больше 0");
        RuleFor(z => new {z.NewValue.InitialCount, z.NewValue.CurrentCount})
            .Must(z => z.InitialCount >= z.CurrentCount)
            .WithMessage("Количество которое было зарезервировано, не может быть меньше текущего.");
        RuleFor(z => z.NewValue.InitialCount)
            .GreaterThan(0)
            .WithMessage("Общее количество для резервации должно быть больше 0");
    }
}

public class EditArticleReservationHandler(IArticleReservation reservationService) : ICommandHandler<EditArticleReservationCommand>
{
    public async Task<Unit> Handle(EditArticleReservationCommand request, CancellationToken cancellationToken)
    {
        await reservationService.EditReservation(request.ReservationId, request.WhoUpdated, request.NewValue, cancellationToken);
        return Unit.Value;
    }
}