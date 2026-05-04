using Analytics.Entities;
using Analytics.Integration.Tests.MockData;
using Analytics.Persistence.Context;
using Localization.Abstractions.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContexts;

namespace Analytics.Integration.Tests.TestContexts;

public class PurchaseFactTestContext(
    DContext context,
    IMediator mediator,
    IScopedStringLocalizer localizer
) : LocalizedTestContext(localizer)
{
    public readonly DContext Context = context;
    public readonly IMediator Mediator = mediator;

    public Currency Currency { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Context.AddMockCurrencies();
        Currency = await Context.Currencies.FirstAsync(cancellationToken);
        await base.InitializeAsync(cancellationToken);
    }
}