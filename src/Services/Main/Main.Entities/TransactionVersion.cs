using Main.Enums;

namespace Main.Entities;

public partial class TransactionVersion
{
    public string Id { get; set; } = null!;

    public Guid TransactionId { get; set; }

    public int CurrencyId { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public decimal TransactionSum { get; set; }

    public TransactionStatus Status { get; set; }

    public DateTime TransactionDatetime { get; set; }

    public int Version { get; set; }

    public DateTime VersionCreatedDatetime { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
