namespace Application.Common.Querying;

public static class LikePattern
{
	public static string Escape(string value)
		=> value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

	public static string Contains(string value) => $"%{Escape(value)}%";

	public static string StartsWith(string value) => $"{Escape(value)}%";
}
