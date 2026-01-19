namespace Main.Entities;

public partial class UserSearchHistory
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string SearchPlace { get; set; } = null!;

    public string Query { get; set; } = null!;

    public DateTime SearchDateTime { get; set; }

    public virtual User User { get; set; } = null!;
}
