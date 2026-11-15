using Main.Enums;

namespace Main.Application.Static;

public static class CacheKeys
{
	public static class OneTimeTokenCache
	{
		public static string OneTimeToken(OneTimeTokenPurpose purpose, Guid tokenId)
			=> $"ont-time-token:{purpose}:{tokenId}";
	}
}
