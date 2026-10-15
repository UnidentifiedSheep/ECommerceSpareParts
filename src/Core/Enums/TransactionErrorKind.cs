namespace Enums;

[Flags]
public enum TransactionErrorKind
{
	None = 0,
	Unknown = 1 << 0,
	ForeignKeyViolation = 1 << 1,
	UniqueConstraintViolation = 1 << 2,
	NotNullViolation = 1 << 3,
	CheckConstraintViolation = 1 << 4,
	ConcurrencyConflict = 1 << 5,
	Deadlock = 1 << 6,
	LockUnavailable = 1 << 7
}
