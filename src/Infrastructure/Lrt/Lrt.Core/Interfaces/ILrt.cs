namespace Application.Common.Interfaces;

public interface ILrt
{
	Task ExecuteAsync(
		Guid jobId,
		Guid leaseHolderId,
		CancellationToken cancellationToken = default);
}

public interface ILrt<in TInputState> : ILrt where TInputState : IInputState
{
}
