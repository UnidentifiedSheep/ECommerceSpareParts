using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

namespace Api.Common.Logging;

public class CustomLogLabelProvider(IList<LokiLabel> lokiLabels) : ILogLabelProvider
{
    public IList<LokiLabel> GetLabels()
    {
        return lokiLabels;
    }

    public IList<string> PropertiesAsLabels { get; } = new List<string> { "traceId", "level" };

    public IList<string> PropertiesToAppend { get; } = new List<string> {  };

    public LokiFormatterStrategy FormatterStrategy => LokiFormatterStrategy.SpecificPropertiesAsLabelsOrAppended;
}