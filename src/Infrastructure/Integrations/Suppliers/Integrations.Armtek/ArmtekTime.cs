namespace Integrations.Armtek;

public static class ArmtekTime
{
	public static readonly TimeSpan Offset = TimeSpan.FromHours(3);

	public static DateTime EndOfDayUtc(DateTimeOffset now)
	{
		var localDate = now.ToOffset(Offset).Date;
		return new DateTimeOffset(localDate.AddDays(1).AddSeconds(-1), Offset).UtcDateTime;
	}
}
