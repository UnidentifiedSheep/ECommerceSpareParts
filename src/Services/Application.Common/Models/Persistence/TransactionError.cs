using Enums;

namespace Application.Common.Models.Persistence;

public sealed record TransactionError(
	Exception Exception,
	TransactionErrorKind Kind = TransactionErrorKind.Unknown,
	string? ConstraintName = null);
