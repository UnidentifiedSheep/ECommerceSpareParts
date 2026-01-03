using Main.Core.Dtos.Anonymous.Articles;

namespace Main.Core.Dtos.Cart;

public class CartItemDto
{
    public int ArticleId { get; set; }
    public int Count { get; set; }
    public DateTime CreatedAt { get; set; }
    public ArticleDto Article { get; set; } = null!;
}