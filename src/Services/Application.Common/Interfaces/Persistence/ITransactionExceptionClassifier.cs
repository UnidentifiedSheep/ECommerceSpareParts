using Application.Common.Models.Persistence;

namespace Application.Common.Interfaces.Persistence;

public interface ITransactionExceptionClassifier
{
	TransactionError Classify(Exception exception);
}
