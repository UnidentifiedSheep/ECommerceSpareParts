using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using IntervalMap.Core.Models;
using IntervalMap.Variations;
using Mapster;
using Pricing.Abstractions.Dtos.Markups;
using Pricing.Abstractions.Models;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Markups.CreateMarkup;

[Transactional]
public record CreateMarkupCommand(
    IEnumerable<NewMarkupRangeDto> Ranges,
    int CurrencyId,
    string? GroupName,
    decimal MarkupRateForUnknownRange) : ICommand<CreateMarkupResult>;

public record CreateMarkupResult(int GroupId);

public class CreateMarkupHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateMarkupCommand, CreateMarkupResult>
{
    public async Task<CreateMarkupResult> Handle(CreateMarkupCommand request, CancellationToken cancellationToken)
    {

        var unknownMarkup = request.MarkupRateForUnknownRange;
        var markupRanges = new List<MarkupRange>();
        var intervalMap = new AdaptiveIntervalMap<MarkupModel>(2, true);
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
                MarkupRate = unknownMarkup
            });
        }

        finalRanges.Add(sortedRanges.Last());
        foreach (var range in finalRanges)
        {
            var markupModel = new MarkupModel(range.MarkupRate);

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
}