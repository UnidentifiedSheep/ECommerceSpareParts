using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

namespace Core.Logger;

public class CustomLogLableProvider : ILogLabelProvider
{
    public IList<LokiLabel> GetLabels()
    {
        return new List<LokiLabel>
        {
            new LokiLabel("app", "monolite-unicorn")
        };
    }
    
    public IList<string> PropertiesAsLabels { get; } = new List<string> { "traceId" };
    
    public IList<string> PropertiesToAppend { get; } = new List<string> { "level" };

    public LokiFormatterStrategy FormatterStrategy { get; } = LokiFormatterStrategy.SpecificPropertiesAsLabelsOrAppended;
}