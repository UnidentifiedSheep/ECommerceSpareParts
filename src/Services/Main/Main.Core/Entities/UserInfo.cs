namespace Main.Core.Entities;

public partial class UserInfo
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public bool IsSupplier { get; set; }

    public string? Description { get; set; }

    public string SearchColumn { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
