using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

namespace Api.Common.Logging;

public class CustomLogLabelProvider(IList<LokiLabel> lokiLabels) : ILogLabelProvider
{
    public IList<LokiLabel> GetLabels()
    {
        return lokiLabels;
    }
    
    public IList<string> PropertiesAsLabels { get; } = new List<string> { "traceId" };
    
    public IList<string> PropertiesToAppend { get; } = new List<string> { "level" };

    public LokiFormatterStrategy FormatterStrategy { get; } = LokiFormatterStrategy.SpecificPropertiesAsLabelsOrAppended;
}