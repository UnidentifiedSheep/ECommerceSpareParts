namespace Main.Modules.Jobs.Application.LRT;

public interface ILrtJobAcquirer
{
	Task<IReadOnlyList<AcquiredJob>> AcquireAsync(
		TimeSpan leaseDuration,
		CancellationToken cancellationToken);
}
