using System.Text.Json;
using StackExchange.Redis;

namespace Cache.Extensions;

public static class RedisValueExtensions
{
	public static T? Deserialize<T>(this RedisValue value)
	{
		if (value.IsNullOrEmpty || !value.HasValue)
			return default;
		return JsonSerializer.Deserialize<T>(value.ToString());
	}
}
