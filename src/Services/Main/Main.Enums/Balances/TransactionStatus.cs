namespace Main.Enums;

[Flags]
public enum TransactionStatus
{
    Pending = 0,
    Completed = 1 << 0,
    CompletionApplied = 1 << 1,
    Reversed = 1 << 2,
    ReversedApplied = 1 << 3
}