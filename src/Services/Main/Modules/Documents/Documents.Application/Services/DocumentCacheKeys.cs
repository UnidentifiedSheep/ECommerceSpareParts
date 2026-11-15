namespace Main.Application.Services;

internal static class DocumentCacheKeys
{
	public static string Link(Guid requestId) => $"document:{requestId}:link";
}
