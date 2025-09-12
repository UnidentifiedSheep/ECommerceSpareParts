namespace Core.Entities;

public partial class TransactionVersion
{
    public string Id { get; set; } = null!;

    public string TransactionId { get; set; } = null!;

    public int CurrencyId { get; set; }

    public string SenderId { get; set; } = null!;

    public string ReceiverId { get; set; } = null!;

    public decimal TransactionSum { get; set; }

    public string Status { get; set; } = null!;

    public DateTime TransactionDatetime { get; set; }

    public int Version { get; set; }

    public DateTime VersionCreatedDatetime { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual AspNetUser Receiver { get; set; } = null!;

    public virtual AspNetUser Sender { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
