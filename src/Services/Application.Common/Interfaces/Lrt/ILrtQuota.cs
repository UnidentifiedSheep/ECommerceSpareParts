namespace Application.Common.LRT;

public interface ILrtQuota : IDisposable
{
    public Guid HolderId { get; }
}