namespace Main.Core.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public int CurrencyId { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public decimal TransactionSum { get; set; }

    public DateTime CreationDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid WhoMadeUserId { get; set; }

    public DateTime TransactionDatetime { get; set; }

    public decimal ReceiverBalanceAfterTransaction { get; set; }

    public decimal SenderBalanceAfterTransaction { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual User? DeletedByNavigation { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual User Receiver { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual User Sender { get; set; } = null!;

    public virtual ICollection<TransactionVersion> TransactionVersions { get; set; } = new List<TransactionVersion>();

    public virtual User WhoMadeUser { get; set; } = null!;
}
