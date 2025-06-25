using System.Drawing;
using Bogus;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.PostGres.Main;

namespace Tests.MockData;

public static class MockData
{
    public const string Locale = "ru";
    public static readonly List<string> ArticleNumbers =
    [
        "505 213", "701 202 412", "AKBA/123-00.2",
        "ARB 199909", "A 152", "213.213-01", "A 909 99 0089",
        "БЕЛОРУСЬАВТО.2133", "БК.21388"
    ];

    public static readonly List<string> ProducerNames =
    [
        "БЕЛ.АВТО", "Sampa", "Febi", "Stellox",
        "Sachs", "Frundel", "JMC", "GTR", "RVI",
        "MB", "MAN", "IVECO", "DAF"
    ];
    
    public static readonly List<string> Colors = Enum.GetNames(typeof(KnownColor)).ToList();

    public static List<NewArticleDto> CreateNewArticleDto(int count)
    {
        var f = new Faker<NewArticleDto>(Locale)
            .RuleFor(x => x.ArticleNumber, f => f.PickRandom(ArticleNumbers))
            .RuleFor(x => x.Name, f => string.Join(" ", f.Lorem.Words(4)))
            .RuleFor(x => x.Description, f => Random.Shared.Next(1, 2) == 1 ? string.Join(" ", f.Lorem.Words(9)) : null)
            .RuleFor(x => x.ProducerId, f => f.PickRandom(f.Random.Int(1, 5)))
            .RuleFor(x => x.Indicator, f => Random.Shared.Next(1, 2) == 1 ? f.PickRandom(Colors) : null)
            .RuleFor(x => x.IsOe, f => f.Random.Bool())
            .RuleFor(x => x.PackingUnit, f => f.PickRandom(3));
        return f.Generate(count);
    }

    public static List<AmwNewProducerDto> CreateNewProducerDto(int count)
    {
        var f = new Faker<AmwNewProducerDto>(Locale)
            .RuleFor(x => x.ProducerName, f => f.PickRandom(ProducerNames))
            .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
            .RuleFor(x => x.IsOe, f => f.Random.Bool());
        return f.Generate(count);
    }
    public static List<AspNetUser> CreateNewUser(int count)
    {
        var f = new Faker<AspNetUser>(Locale)
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.NormalizedEmail, f => f.Person.Email.ToUpperInvariant())
            .RuleFor(x => x.PhoneNumber, f => f.Person.Phone)
            .RuleFor(x => x.Name, f => f.Person.FirstName)
            .RuleFor(x => x.Surname, f => f.Person.LastName)
            .RuleFor(x => x.UserName, f => f.Person.UserName)
            .RuleFor(x => x.Id, f => f.Person.UserName)
            .RuleFor(x => x.NormalizedUserName, f => f.Person.UserName.ToUpperInvariant())
            .RuleFor(x => x.PhoneNumberConfirmed, f => f.Random.Bool())
            .RuleFor(x => x.EmailConfirmed, f => f.Random.Bool())
            .RuleFor(x => x.LockoutEnabled, f => f.Random.Bool())
            .RuleFor(x => x.TwoFactorEnabled, f => f.Random.Bool());
        return f.Generate(count);
    }
}