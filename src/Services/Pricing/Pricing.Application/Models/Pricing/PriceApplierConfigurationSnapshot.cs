using System.Text.Json;
using Application.Common.Services;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Enums;

namespace Pricing.Application.Models.Pricing;

public sealed record PriceApplierConfigurationSnapshot(
    IReadOnlyList<PriceApplierDto> Appliers,
    string Version)
{
    public static PriceApplierConfigurationSnapshot Create(
        IReadOnlyList<PriceApplierDto> persistedAppliers,
        IReadOnlyCollection<ApplierNamedObjectBase> localAppliers)
    {
        var appliers = MergeWithLocalAppliers(
            persistedAppliers,
            localAppliers);
        var version = ConfigurationVersionGenerator.Generate(writer =>
            WriteConfiguration(writer, appliers));

        return new PriceApplierConfigurationSnapshot(appliers, version);
    }

    private static IReadOnlyList<PriceApplierDto> MergeWithLocalAppliers(
        IReadOnlyList<PriceApplierDto> persistedAppliers,
        IReadOnlyCollection<ApplierNamedObjectBase> localAppliers)
    {
        var persistedBySystemName = persistedAppliers
            .ToDictionary(x => x.SystemName);
        var result = new List<PriceApplierDto>(
            persistedAppliers.Count + localAppliers.Count);

        foreach (var local in localAppliers)
        {
            persistedBySystemName.Remove(
                local.SystemName,
                out var persisted);
            var states = GetSupportedUsages(local)
                .Select(usage => CreateLocalState(
                    local,
                    usage,
                    persisted))
                .ToList();

            if (states.Count == 0) continue;

            result.Add(new PriceApplierDto
            {
                SystemName = local.SystemName,
                Name = persisted?.Name ?? local.SystemName,
                IsDynamic = false,
                DslLogic = null,
                States = states
            });
        }

        result.AddRange(persistedBySystemName.Values.Where(x => x.IsDynamic));
        return result;
    }

    private static PriceApplierStateDto CreateLocalState(
        ApplierNamedObjectBase local,
        PriceOfferSourceType usage,
        PriceApplierDto? persisted)
    {
        var persistedState = persisted?.States
            .FirstOrDefault(x => x.Usage == usage);

        return new PriceApplierStateDto
        {
            PriceApplierSystemName = local.SystemName,
            Usage = usage,
            Order = local.Order,
            Enabled = persisted is null || persistedState?.Enabled == true
        };
    }

    private static IEnumerable<PriceOfferSourceType> GetSupportedUsages(
        ApplierNamedObjectBase local)
    {
        if (local is ISupplierPriceApplier)
            yield return PriceOfferSourceType.Supplier;

        if (local is IInternalPriceApplier)
            yield return PriceOfferSourceType.OurWarehouse;
    }

    private static void WriteConfiguration(
        Utf8JsonWriter writer,
        IReadOnlyList<PriceApplierDto> appliers)
    {
        writer.WriteStartArray();

        foreach (var item in appliers.OrderBy(
                     x => x.SystemName,
                     StringComparer.Ordinal))
        {
            writer.WriteStartObject();
            writer.WriteString("systemName", item.SystemName);
            writer.WriteString("dslLogic", item.DslLogic);
            writer.WriteStartArray("states");

            foreach (var state in item.States.OrderBy(x => x.Usage))
            {
                writer.WriteStartObject();
                writer.WriteNumber("usage", (int)state.Usage);
                writer.WriteNumber("order", state.Order);
                writer.WriteBoolean("enabled", state.Enabled);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
