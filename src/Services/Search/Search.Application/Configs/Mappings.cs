using Search.Abstractions.Dtos;
using Search.Entities;

namespace Search.Application.Configs;

public static class Mappings
{
    public static Article ToArticle(this ArticleDto dto)
    {
        return new Article(dto.Id, dto.ArticleNumber, dto.Title, dto.ProducerId, dto.ProducerName, dto.Popularity);
    }
    
    public static List<Article> ToArticles(this IEnumerable<ArticleDto> dtos) => dtos.Select(ToArticle).ToList();

    public static ArticleDto ToDto(this Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            ArticleNumber = article.ArticleNumber,
            Title = article.Title,
            ProducerName = article.ProducerName,
            ProducerId = article.ProducerId,
            Popularity = article.Popularity
        };
    }
    
    public static List<ArticleDto> ToDtos(this IEnumerable<Article> articles) => articles.Select(ToDto).ToList();
}