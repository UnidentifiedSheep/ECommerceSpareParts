namespace Application.Common.Interfaces.Persistence;

public interface IApplicationTransactionFactory
{
	IApplicationTransaction Create();
}
