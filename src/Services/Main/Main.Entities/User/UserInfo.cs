using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.User;

public class UserInfo : Entity<UserInfo, Guid>, ILinqEntity<UserInfo, Guid>
{
    private UserInfo() { }

    private UserInfo(
        Guid userId,
        string name,
        string surname,
        string? description)
    {
        UserId = userId;
        SetName(name);
        SetSurname(surname);
        SetDescription(description);
    }

    public Guid UserId { get; }
    public string Name { get; private set; } = null!;
    public string Surname { get; private set; } = null!;
    public string? Description { get; private set; }
    public string SearchColumn { get; private set; } = null!;

    public static Expression<Func<UserInfo, Guid>> GetKeySelector() { return x => x.UserId; }

    public static Expression<Func<UserInfo, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.UserId == key;
    }

    internal static UserInfo Create(
        Guid userId,
        string name,
        string surname,
        string? description)
    {
        return new UserInfo(
            userId,
            name,
            surname,
            description);
    }

    public void SetName(string name)
    {
        Name = name.Trim()
            .EnsureNotNullOrWhiteSpace("user.name.required")
            .EnsureMinLength(3, "user.name.min.length")
            .Ensure(x => x.All(c => !char.IsSymbol(c)), "user.name.no.special.chars")
            .EnsureMaxLength(30, "user.name.max.length");
        UpdateSearchColumn();
    }

    public void SetSurname(string surname)
    {
        Surname = surname.Trim()
            .EnsureNotNullOrWhiteSpace("user.surname.required")
            .EnsureMinLength(3, "user.surname.min.length")
            .Ensure(x => x.All(c => !char.IsSymbol(c)), "user.surname.no.special.chars")
            .EnsureMaxLength(30, "user.surname.max.length");
        UpdateSearchColumn();
    }

    public void SetDescription(string? description)
    {
        Description = description
            .NullIfWhiteSpace()
            ?.EnsureMaxLength(300, "user.description.max.length");
        UpdateSearchColumn();
    }

    internal void Update(
        string name,
        string surname,
        string? description)
    {
        SetName(name);
        SetSurname(surname);
        SetDescription(description);
    }

    private void UpdateSearchColumn() { SearchColumn = $"{Name} {Surname} {Description}".ToUpperInvariant(); }

    public override Guid GetId() { return UserId; }
}
