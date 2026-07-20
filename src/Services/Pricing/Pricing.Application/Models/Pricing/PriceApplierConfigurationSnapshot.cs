using System.Text.Json;
using Application.Common.Services;
using Pricing.Application.Dtos.PriceApplier;

namespace Pricing.Application.Models.Pricing;

public sealed record PriceApplierConfigurationSnapshot(
    IReadOnlyList<PriceApplierDto> Appliers,
    string Version)
{
    public static PriceApplierConfigurationSnapshot Create(
        IReadOnlyList<PriceApplierDto> appliers)
    {
        var version = ConfigurationVersionGenerator.Generate(writer =>
            WriteConfiguration(writer, appliers));

        return new PriceApplierConfigurationSnapshot(appliers, version);
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
