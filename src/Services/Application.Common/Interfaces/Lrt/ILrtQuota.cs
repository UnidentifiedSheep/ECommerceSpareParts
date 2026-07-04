namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuota : IDisposable
{
    public Guid HolderId { get; }
}