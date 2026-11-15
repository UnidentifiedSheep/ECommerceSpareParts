namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuota : IDisposable
{
	Guid HolderId { get; }
}
