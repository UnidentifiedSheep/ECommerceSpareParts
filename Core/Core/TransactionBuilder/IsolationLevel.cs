namespace Core.TransactionBuilder;

public enum IsolationLevel
{
    ReadCommitted = 4096,
    ReadUncommitted = 256,
    RepeatableRead = 65536,
    Serializable = 1048576,
    Unspecified = -1
}