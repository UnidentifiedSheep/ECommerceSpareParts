namespace Search.Entities;

public record Product(
    int Id,
    string Sku,
    string NormalizedSku,
    string Title,
    int ProducerId,
    string ProducerName,
    long Popularity);