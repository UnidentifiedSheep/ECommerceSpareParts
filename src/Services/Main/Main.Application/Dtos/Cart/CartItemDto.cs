using Main.Abstractions.Dtos.Anonymous.Articles;

namespace Main.Abstractions.Dtos.Cart;

public class CartItemDto
{
    public int ArticleId { get; set; }
    public int Count { get; set; }
    public DateTime CreatedAt { get; set; }
    public ArticleDto Article { get; set; } = null!;
}