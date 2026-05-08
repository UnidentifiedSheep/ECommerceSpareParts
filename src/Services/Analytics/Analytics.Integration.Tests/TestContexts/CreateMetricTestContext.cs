using Abstractions.Interfaces;
using Analytics.Entities;
using Analytics.Integration.Tests.MockData;
using Analytics.Persistence.Context;
using Localization.Abstractions.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContexts;

namespace Analytics.Integration.Tests.TestContexts;

public class CreateMetricTestContext(
    IScopedStringLocalizer localizer,
    DContext context,
    IMediator mediator,
    IJsonSerializer serializer) : LocalizedTestContext(localizer)
{
    public Currency Currency = null!;
    public IMediator Mediator => mediator;
    public DContext Context => context;
    public IJsonSerializer Serializer => serializer;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await base.InitializeAsync(cancellationToken);

        await Context.AddMockCurrencies();
        Currency = await Context.Currencies.FirstAsync(cancellationToken);
    }
}