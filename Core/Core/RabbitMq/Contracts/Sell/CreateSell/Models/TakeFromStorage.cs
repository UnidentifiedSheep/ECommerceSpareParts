namespace Core.RabbitMq.Contracts.Sell.CreateSell.Models;

public class TakeFromStorage
{
    public int StorageContentId { get; set; }
    public int TakenCount { get; set; }
}