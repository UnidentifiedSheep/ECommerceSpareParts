using Main.Enums;

namespace Main.Abstractions.Dtos.ArticleSizes;

public class ArticleSizeDto
{
    public int ArticleId { get; set; }

    public decimal Length { get; set; }

    public decimal Width { get; set; }

    public decimal Height { get; set; }

    public DimensionUnit Unit { get; set; }

    public decimal VolumeM3 { get; set; }
}