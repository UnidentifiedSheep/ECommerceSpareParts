

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
                    .GreaterThan(0)
                    .When(z => z.GivenPrice.HasValue)
                    .WithMessage("Предложенная цена должна быть больше 0");

                x.RuleFor(z => z.InitialCount)
                    .GreaterThan(0)
                    .WithMessage("Общее количество для резервации должно быть больше 0");
                
                x.RuleFor(z => z.CurrentCount)
                    .GreaterThan(0)
                    .WithMessage("Текущее количество для резервации должно быть больше 0");

                x.RuleFor(z => z.InitialCount)
                    .GreaterThanOrEqualTo(z => z.CurrentCount)
                    .WithMessage("Начальное количество не может быть меньше текущего количества");
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