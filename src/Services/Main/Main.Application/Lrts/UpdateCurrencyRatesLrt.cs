using Abstractions;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Domain.CommonEntities;
using Main.Application.Handlers.Currencies.UpdateCurrenciesRates;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts;

public class UpdateCurrencyRatesLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    ILogger<UpdateCurrencyRatesLrt> logger,
    ISender sender
) : LrtNamedObjectBase(
    jobRepository,
    unitOfWork,
    publisher,
    logger)
{
    protected override IServiceDefinition ServiceDefinition => ServicesDefinitions.Main;
    public override Type InputType => typeof(NoneInputState);
    public override Type StateType => typeof(NoneInputState);
    public override string SystemName => nameof(UpdateCurrencyRatesLrt);
    public override string NameLocalizationKey => "lrt.currency.rates.update.name";
    public override string DescriptionLocalizationKey => "lrt.currency.rates.update.description";

    protected override Task DoWork()
    {
        return sender.Send(new UpdateCurrenciesRatesCommand(), CancellationToken);
    }
}