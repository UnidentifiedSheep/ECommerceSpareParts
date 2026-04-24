using Domain;
using Domain.Extensions;

namespace Main.Entities.User;

public class UserInfo : Entity<UserInfo, Guid>
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Surname { get; private set; } = null!;
    public string? Description { get; private set; }
    public string SearchColumn { get; private set; } = null!;

    private UserInfo() { }

    private UserInfo(Guid userId, string name, string surname, string? description)
    {
        UserId = userId;
        SetName(name);
        SetSurname(surname);
        SetDescription(description);
    }

    internal static UserInfo Create(Guid userId, string name, string surname, string? description)
    {
        return new UserInfo(userId, name, surname, description);
    }

    public void SetName(string name)
    {
        Name = name.Trim()
                .AgainstNullOrWhiteSpace("user.name.required")
                .AgainstTooShort(3, "user.name.min.length")
                .Against(x => x.Any(char.IsSymbol), "user.name.no.special.chars")
                .AgainstTooLong(30, "user.name.max.length");
        UpdateSearchColumn();
    }
    
    public void SetSurname(string surname)
    {
        Surname = surname.Trim()
            .AgainstNullOrWhiteSpace("user.surname.required")
            .AgainstTooShort(3, "user.surname.min.length")
            .Against(x => x.Any(char.IsSymbol), "user.surname.no.special.chars")
            .AgainstTooLong(30, "user.surname.max.length");
        UpdateSearchColumn();
    }

    public void SetDescription(string? description)
    {
        Description = description
            .NullIfWhiteSpace()
            ?.AgainstTooLong(300, "user.description.max.length");
        UpdateSearchColumn();
    }

    internal void Update(string name, string surname, string? description)
    {
        SetName(name);
        SetSurname(surname);
        SetDescription(description);
    }

    private void UpdateSearchColumn()
    {
        SearchColumn = $"{Name} {Surname} {Description}".ToUpperInvariant();
    }

    public override Guid GetId() => UserId;
}