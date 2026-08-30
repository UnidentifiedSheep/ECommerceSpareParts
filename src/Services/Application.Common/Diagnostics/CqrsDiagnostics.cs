using System.Diagnostics;

namespace Application.Common.Diagnostics;

public static class CqrsDiagnostics
{
	public const string ActivitySourceName = "Application.Cqrs";

	public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
