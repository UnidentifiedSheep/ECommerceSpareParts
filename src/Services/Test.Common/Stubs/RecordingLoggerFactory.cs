using Microsoft.Extensions.Logging;

namespace Tests.Stubs;

public sealed class RecordingLoggerFactory : ILoggerFactory
{
	public List<LogLevel> LogLevels { get; } = [];

	public ILogger CreateLogger(string categoryName) => new RecordingLogger(LogLevels);

	public void AddProvider(ILoggerProvider provider) { }

	public void Dispose() { }

	private sealed class RecordingLogger(List<LogLevel> logLevels) : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			logLevels.Add(logLevel);
		}
	}
}
