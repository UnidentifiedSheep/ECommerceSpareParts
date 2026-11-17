using Exceptions.Base.Localized;

namespace Main.Entities.Analytics.Exceptions;

public sealed class AnalyticsRunNotFoundException(Guid runId) : LocalizedNotFoundException(
	AnalyticsRunNotFoundMessage.Instance,
	new
	{
		RunId = runId
	});
