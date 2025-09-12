namespace Core.Entities;

public partial class UserSearchHistory
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string SearchPlace { get; set; } = null!;

    public string Query { get; set; } = null!;

    public DateTime SearchDateTime { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
