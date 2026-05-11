using Search.Abstractions.Dtos;
using Search.Entities;

namespace Search.Application.Configs;

public static class Mappings
{
    public static Product ToArticle(this ArticleDto dto)
    {
        throw new NotImplementedException();
        //return new Product(dto.Id, dto.ArticleNumber, dto.Title, dto.ProducerId, dto.ProducerName, dto.Popularity);
    }

    public static List<Product> ToArticles(this IEnumerable<ArticleDto> dtos)
    {
        return dtos.Select(ToArticle).ToList();
    }

    public static ArticleDto ToDto(this Product product)
    {
        return new ArticleDto
        {
            Id = product.Id,
            ArticleNumber = product.Sku,
            Title = product.Title,
            ProducerName = product.ProducerName,
            ProducerId = product.ProducerId,
            Popularity = product.Popularity
        };
    }

    public static List<ArticleDto> ToDtos(this IEnumerable<Product> articles)
    {
        return articles.Select(ToDto).ToList();
    }

    /*public static Product ToArticle(this ContractArticle article)
    {
        throw new NotImplementedException();
        //return new Product(article.Id, article.ArticleNumber, article.ArticleName, article.ProducerId,
            //article.ProducerName, article.Popularity);
    }

    public static List<Product> ToArticles(this IEnumerable<ContractArticle> articles)
    {
        return articles.Select(ToArticle).ToList();
    }*/
}