using Microsoft.Extensions.Logging;

namespace Persistence.Logging;

public static class DatabaseEvents
{
    public static readonly EventId DatabaseEnsuredCreatedEventId = new(1001, nameof(DatabaseEnsuredCreatedEventId));
    
    public static readonly Action<ILogger, string, Exception?> DatabaseEnsuredCreated =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            DatabaseEnsuredCreatedEventId,
            "Ensured that data base created for {ContextName}"
        );
}