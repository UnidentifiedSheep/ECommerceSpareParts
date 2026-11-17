namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuotaLease : IDisposable
{
	Guid HolderId { get; }
}
