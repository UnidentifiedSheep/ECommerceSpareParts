using Application.Common.Interfaces;
using Main.Application.Extensions;
using Core.Attributes;
using Core.Dtos.Amw.Markups;
using Core.Entities;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Core.Models;
using IntervalMap.Core.Models;
using IntervalMap.Variations;
using Mapster;

namespace Main.Application.Handlers.Markups.CreateMarkup;

[Transactional]
public record CreateMarkupCommand(
    IEnumerable<NewMarkupRangeDto> Ranges,
    int CurrencyId,
    string? GroupName,
    decimal MarkupForUnknownRange) : ICommand<CreateMarkupResult>;

public record CreateMarkupResult(int GroupId);

public class CreateMarkupHandler(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
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

            //Check if we can build interval map with this data
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
        await currencyRepository.EnsureCurrenciesExists([currencyId], cancellationToken);
    }
}