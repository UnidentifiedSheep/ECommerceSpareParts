using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Core.Models;
using IntervalMap.Core.Models;
using IntervalMap.Variations;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.Markups;
using Main.Core.Entities;
using Mapster;

namespace Main.Application.Handlers.Markups.CreateMarkup;

[Transactional]
public record CreateMarkupCommand(
    IEnumerable<NewMarkupRangeDto> Ranges,
    int CurrencyId,
    string? GroupName,
    decimal MarkupForUnknownRange) : ICommand<CreateMarkupResult>;

public record CreateMarkupResult(int GroupId);

public class CreateMarkupHandler(DbDataValidatorBase dbValidator, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMarkupCommand, CreateMarkupResult>
{
    public async Task<CreateMarkupResult> Handle(CreateMarkupCommand request, CancellationToken cancellationToken)
    {
        await ValidateData(request.CurrencyId, cancellationToken);

        var unknownMarkup = request.MarkupForUnknownRange;
        var markupRanges = new List<MarkupRange>();
        var intervalMap = new AdaptiveIntervalMap<MarkupModel>();
        var sortedRanges = request.Ranges
            .OrderBy(r => r.RangeStart)
            .ToList();
        var finalRanges = new List<NewMarkupRangeDto>();

        for (var i = 0; i < sortedRanges.Count - 1; i++)
        {
            var curr = sortedRanges[i];
            var next = sortedRanges[i + 1];

            finalRanges.Add(curr);

            var nextRangeStart = Math.Round(curr.RangeEnd + 0.01, 2);
            var nextRangeEnd = Math.Round(next.RangeStart - 0.01, 2);

            if (!(nextRangeStart < nextRangeEnd)) continue;

            finalRanges.Add(new NewMarkupRangeDto
            {
                RangeStart = nextRangeStart,
                RangeEnd = nextRangeEnd,
                Markup = unknownMarkup
            });
        }

        finalRanges.Add(sortedRanges.Last());
        foreach (var range in finalRanges)
        {
            var markupModel = new MarkupModel((double)range.Markup);

            //Check if we can build an interval map with this data
            intervalMap.AddInterval(new Interval<MarkupModel>(range.RangeStart, range.RangeEnd, markupModel));
            markupRanges.Add(range.Adapt<MarkupRange>());
        }

        var group = new MarkupGroup
        {
            Name = request.GroupName,
            CurrencyId = request.CurrencyId,
            MarkupRanges = markupRanges
        };
        await unitOfWork.AddAsync(group, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateMarkupResult(group.Id);
    }

    private async Task ValidateData(int currencyId, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan().EnsureCurrencyExists(currencyId);
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}