namespace Application.Common.Models.Lrt;

public sealed class LrtStepDefinition
{
	internal LrtStepDefinition(
		string key,
		string systemName,
		int? maxAttempts,
		Type inputType,
		Func<LrtStepInputContext, CancellationToken, Task<string>> inputFactory)
	{
		Key = key;
		SystemName = systemName;
		MaxAttempts = maxAttempts;
		InputType = inputType;
		InputFactory = inputFactory;
	}

	public string Key { get; }
	public string SystemName { get; }
	public int? MaxAttempts { get; }
	internal Type InputType { get; }
	internal Func<LrtStepInputContext, CancellationToken, Task<string>> InputFactory { get; }
	internal List<LrtStepDefinition> Dependencies { get; } = [];
}
