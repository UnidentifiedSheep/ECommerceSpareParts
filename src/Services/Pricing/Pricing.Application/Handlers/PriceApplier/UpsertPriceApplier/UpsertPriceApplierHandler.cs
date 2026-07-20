using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Exceptions;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Projections;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Entities.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Handlers.PriceApplier.UpsertPriceApplier;

[Diagnostics]
[Transactional, AutoSave]
public record UpsertPriceApplierCommand(
    string SystemName,
    string? DslLogic,
    IReadOnlyList<UpsertPriceApplierStateDto> States
    ) : ICommand<UpsertPriceApplierResult>;
public record UpsertPriceApplierResult(PriceApplierDto Applier);

public class UpsertPriceApplierHandler(
    INamedObjectRegistry<ApplierNamedObjectBase> registry,
    IRepository<Entities.Pricing.PriceApplier, string> repository,
    IReadRepository<PriceApplierState, PriceApplierStateKey> stateRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpsertPriceApplierCommand, UpsertPriceApplierResult>
{
    public async Task<UpsertPriceApplierResult> Handle(
        UpsertPriceApplierCommand request, 
        CancellationToken cancellationToken)
    {
        var local = registry.TryGetBySystemName(request.SystemName);
        var criteria = Criteria<Entities.Pricing.PriceApplier>
            .New()
            .Where(x => x.SystemName == request.SystemName)
            .Include(x => x.States)
            .Track()
            .Build();
        
        var model = await repository.FirstOrDefaultAsync(criteria, cancellationToken);
        var created = model is null;
        var dslLogic = local is null
            ? GetRequiredDslLogic(request.DslLogic)
            : null;

        if (dslLogic is not null
            && !await DynamicApplierDslValidator.IsValidAsync(
                dslLogic,
                cancellationToken))
            throw new InvalidInputException("price.applier.dsl.logic.invalid");

        if (model is not null)
            EnsureKindMatches(model, local);

        await EnsureOrdersAreAvailableAsync(
            request.SystemName,
            request.States,
            local,
            cancellationToken);

        if (model is null)
        {
            model = local is null
                ? Entities.Pricing.PriceApplier.Create(
                    request.SystemName,
                    dslLogic!)
                : Entities.Pricing.PriceApplier.CreateLocal(request.SystemName);
            
            await unitOfWork.AddAsync(model, cancellationToken);
        }

        if (!created && local is null)
            model.SetDslLogic(dslLogic!);

        foreach (var state in request.States)
            UpdateState(model, state, GetOrder(state, local));

        model.RemoveStatesExcept(request.States.Select(x => x.Usage));
        
        return new UpsertPriceApplierResult(PriceApplierProjections.ToApplierDto.AsFunc()(model));
    }

    private static void UpdateState(
        Entities.Pricing.PriceApplier applier,
        UpsertPriceApplierStateDto stateDto,
        int order)
    {
        var state = applier
            .States
            .FirstOrDefault(x => x.Usage == stateDto.Usage);

        if (state == null)
            applier.AddState(PriceApplierState.Create(
                applier.SystemName,
                stateDto.Usage,
                order,
                stateDto.Enabled));
        else
            state.Update(order, stateDto.Enabled);
    }

    private static string GetRequiredDslLogic(string? dslLogic)
    {
        return string.IsNullOrWhiteSpace(dslLogic)
            ? throw new InvalidInputException("price.applier.dsl.logic.required")
            : dslLogic;
    }

    private static int GetLocalOrder(
        ApplierNamedObjectBase local,
        PriceOfferSourceType usage)
    {
        var supportsUsage = usage switch
        {
            PriceOfferSourceType.OurWarehouse => local is IInternalPriceApplier,
            PriceOfferSourceType.Supplier => local is ISupplierPriceApplier,
            _ => false
        };

        return !supportsUsage
            ? throw new InvalidInputException("price.applier.usage.not.supported")
            : local.Order;
    }

    private static int GetOrder(
        UpsertPriceApplierStateDto state,
        ApplierNamedObjectBase? local)
    {
        return local is null
            ? state.Order
              ?? throw new InvalidInputException("price.applier.order.required")
            : GetLocalOrder(local, state.Usage);
    }

    private static void EnsureKindMatches(
        Entities.Pricing.PriceApplier model,
        ApplierNamedObjectBase? local)
    {
        if ((local is null) == (model.DslLogic is null))
            throw new InvalidInputException("price.applier.system.name.conflict");
    }

    private async Task EnsureOrdersAreAvailableAsync(
        string systemName,
        IReadOnlyList<UpsertPriceApplierStateDto> states,
        ApplierNamedObjectBase? local,
        CancellationToken cancellationToken)
    {
        foreach (var state in states)
        {
            var order = GetOrder(state, local);
            if (!state.Enabled) continue;

            var orderIsOccupied = await stateRepository.Query.AnyAsync(
                x => x.Enabled
                     && x.PriceApplierSystemName != systemName
                     && x.Usage == state.Usage
                     && x.Order == order,
                cancellationToken);

            if (orderIsOccupied)
                throw new InvalidInputException("price.applier.order.duplicate");
        }
    }

}
