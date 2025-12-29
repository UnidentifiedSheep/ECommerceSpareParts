using Microsoft.Extensions.Logging;

namespace Persistence.Logging;

public static class SeedEvents
{
    public static readonly EventId NoSeedsFoundEventId = new(1000, nameof(NoSeedsFoundEventId));
    public static readonly EventId SeedStartedEventId = new(1001, nameof(SeedStartedEventId));
    public static readonly EventId SeedCompletedEventId = new(1002, nameof(SeedCompletedEventId));
    
    public static readonly Action<ILogger, string, Exception?> NoSeedsFound =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            NoSeedsFoundEventId,
            "No seeds found for {ContextName}"
        );
    
    public static readonly Action<ILogger, string, Exception?> SeedStarted =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            SeedStartedEventId,
            "Seeding started for {ContextName}"
        );
    
    public static readonly Action<ILogger, string, Exception?> SeedCompleted =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            SeedCompletedEventId,
            "Seeding completed for {ContextName}"
        );
}