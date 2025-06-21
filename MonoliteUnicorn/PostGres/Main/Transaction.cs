using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class Transaction
{
    public string Id { get; set; } = null!;

    public int CurrencyId { get; set; }

    public string SenderId { get; set; } = null!;

    public string ReceiverId { get; set; } = null!;

    public decimal TransactionSum { get; set; }

    public DateTime CreationDate { get; set; }

    public string Status { get; set; } = null!;

    public string WhoMadeUserId { get; set; } = null!;

    public DateTime TransactionDatetime { get; set; }

    public decimal ReceiverBalanceAfterTransaction { get; set; }

    public decimal SenderBalanceAfterTransaction { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual AspNetUser? DeletedByNavigation { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual AspNetUser Receiver { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual AspNetUser Sender { get; set; } = null!;

    public virtual ICollection<TransactionVersion> TransactionVersions { get; set; } = new List<TransactionVersion>();

    public virtual AspNetUser WhoMadeUser { get; set; } = null!;
}
