using System.Text.Json.Serialization;
using Attributes;
using Attributes.JsonAttributes;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record MetricTimer
{
    //for json serializer
    public MetricTimer()
    {
    }

    public MetricTimer(DateTime start, DateTime end)
    {
        StartTime = start;
        EndTime = end;
    }

    [JsonPropertyName("start_time")]
    [LocalizedJsonFieldName("start_time")]
    public DateTime StartTime { get; init; }

    [JsonPropertyName("end_time")]
    [LocalizedJsonFieldName("end_time")]
    public DateTime EndTime { get; init; }
}