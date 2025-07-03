using Core.Interface;
using FluentValidation;
using IntervalMap.Enums;
using IntervalMap.IntervalVariations;
using IntervalMap.Models;
using Mapster;
using MonoliteUnicorn.Dtos.Amw.Markups;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator.Models;

namespace MonoliteUnicorn.EndPoints.Markups.CreateMarkup;

public record CreateMarkupCommand(IEnumerable<NewMarkupRangeDto> Ranges, int CurrencyId, string? GroupName) : ICommand<CreateMarkupResult>;
public record CreateMarkupResult(int GroupId);

public class CreateMarkupValidation : AbstractValidator<CreateMarkupCommand>
{
    public CreateMarkupValidation()
    {
        RuleFor(x => x.Ranges)
            .NotEmpty()
            .WithMessage("Диапазоны не могут быть пустыми");

        RuleForEach(x => x.Ranges).ChildRules(context =>
        {
            context.RuleFor(x => x.Markup)
                .Must(x => Math.Round(x, 2) > 0)
                .WithMessage("Наценка не может быть меньше или равна 0");
        });
    }
}

public class CreateMarkupHandler(DContext context) : ICommandHandler<CreateMarkupCommand, CreateMarkupResult>
{
    public async Task<CreateMarkupResult> Handle(CreateMarkupCommand request, CancellationToken cancellationToken)
    {
        await context.EnsureCurrencyExists(request.CurrencyId, cancellationToken);
        var markupRanges = new List<MarkupRange>();
        var intervalMap = new AdaptiveIntervalMap<MarkupModel>();
        var sortedRanges = request.Ranges
            .OrderBy(r => r.RangeStart)
            .ToList();
        var finalRanges = new List<NewMarkupRangeDto>();

        for (int i = 0; i < sortedRanges.Count - 1; i++)
        {
            var curr = sortedRanges[i];
            var next = sortedRanges[i + 1];

            finalRanges.Add(curr);

            if (!(Math.Round(curr.RangeEnd, 2) < Math.Round(next.RangeStart, 2))) continue;
            var start = Math.Round(curr.RangeEnd + 0.01, 2);
            var end = Math.Round(next.RangeStart - 0.01, 2);
            
            if (!(start < end)) continue;
            var avgMarkup = Math.Round((curr.Markup + next.Markup) / 2.0, 2);
            finalRanges.Add(new NewMarkupRangeDto
            {
                RangeStart = start,
                RangeEnd = end,
                Markup = avgMarkup
            });
        }

        finalRanges.Add(sortedRanges.Last());
        foreach (var range in finalRanges)
        {
            range.Markup = Math.Round(range.Markup, 2);
            range.RangeStart = Math.Round(range.RangeStart, 2);
            range.RangeEnd = Math.Round(range.RangeEnd, 2);
            var markupModel = new MarkupModel(range.Markup);
            intervalMap.AddInterval(new Interval<MarkupModel>(range.RangeStart, range.RangeEnd, markupModel));
            markupRanges.Add(range.Adapt<MarkupRange>());
        }
        

        var group = new MarkupGroup
        {
            Name = request.GroupName,
            CurrencyId = request.CurrencyId,
            MarkupRanges = markupRanges
        };
        await context.AddAsync(group, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return new CreateMarkupResult(group.Id);
    }
}