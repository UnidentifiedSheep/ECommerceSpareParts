namespace Core.Entities;

public partial class UserInfo
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public bool IsSupplier { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
