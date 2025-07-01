

using Core.Interface;
using FluentValidation;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.ArticleReservations;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.CreateArticleReservetions;

public record CreateArticleReservationCommand(IEnumerable<NewArticleReservationDto> Reservations, string WhoCreated) : ICommand;

public class CreateArticleReservationValidation : AbstractValidator<CreateArticleReservationCommand>
{
    public CreateArticleReservationValidation()
    {
        RuleForEach(x => x.Reservations)
            .ChildRules(x =>
            {
                x.RuleFor(z => z.GivenPrice)
                    .Must(z => Math.Round(z!.Value, 2) > 0)
                    .When(z => z.GivenPrice != null)
                    .WithMessage("Предложенная цена должна быть больше 0");
                x.RuleFor(z => new {z.InitialCount, z.CurrentCount})
                    .Must(z => z.InitialCount > z.CurrentCount)
                    .WithMessage("Количество которое было зарезервировано, не может быть меньше или равно текущему, при создании резервации.");
                x.RuleFor(z => z.InitialCount)
                    .GreaterThan(0)
                    .WithMessage("Общее количество для резервации должно быть больше 0");
            });
    }
}

public class CreateArticleReservationHandler(IArticleReservation articleReservations) : ICommandHandler<CreateArticleReservationCommand>
{
    public async Task<Unit> Handle(CreateArticleReservationCommand request, CancellationToken cancellationToken)
    {
        await articleReservations.CreateReservation(request.Reservations, request.WhoCreated, cancellationToken);
        return Unit.Value;
    }
}