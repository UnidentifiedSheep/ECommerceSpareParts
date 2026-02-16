namespace Search.Entities;

public record Article(int Id, string ArticleNumber, string Title, int ProducerId, string ProducerName, long Popularity);