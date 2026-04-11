using BulkValidation.Core.Attributes;
using Main.Enums;

namespace Main.Entities;

public class Transaction
{
    [Validate]
    public Guid Id { get; set; }

    public int CurrencyId { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public decimal TransactionSum { get; set; }

    public DateTime CreationDate { get; set; }

    public TransactionStatus Status { get; set; }

    public Guid WhoMadeUserId { get; set; }

    public DateTime TransactionDatetime { get; set; }

    public decimal ReceiverBalanceAfterTransaction { get; set; }

    public decimal SenderBalanceAfterTransaction { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}