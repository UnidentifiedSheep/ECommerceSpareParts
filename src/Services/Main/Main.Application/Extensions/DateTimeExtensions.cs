namespace Main.Application.Extensions;

public static class DateTimeExtensions
{
    public static DateTime WithRandomTicks(this DateTime dateTime)
    {
        return dateTime.AddTicks(Random.Shared.Next(10, 1000));
    }
}