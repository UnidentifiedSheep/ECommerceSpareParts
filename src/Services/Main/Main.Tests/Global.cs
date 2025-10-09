using Bogus;

namespace Tests;

public static class Global
{
    public const string Locale = "ru";
    public static Faker Faker = new(Locale);
}