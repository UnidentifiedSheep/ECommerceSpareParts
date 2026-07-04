namespace Main.Enums.Balances;

[Flags]
public enum TransactionStatus
{
    Pending = 0,
    Completed = 1 << 0,
    CompletionApplied = 1 << 1,
    Reversed = 1 << 2,
    ReversedApplied = 1 << 3,
    CompletionProfileApplied = 1 << 4,
    ReversalProfileApplied = 1 << 5
}