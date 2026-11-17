using Application.Common;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Lrt;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;

namespace Tests.Stubs;

public sealed class QuotaTestLrt : ILrtNamedObject<NoneInputState>, IQuotaBoundLrt
{
	public const string LrtName = "test-quota-lrt";
	public const string QuotaName = "test-isolated-quota";

	public string SystemName => LrtName;

	public ILocalizableMessage NameLocalizationMessage => new LocalizableMessage("test-quota-lrt.name");

	public ILocalizableMessage DescriptionLocalizationMessage
		=> new LocalizableMessage("test-quota-lrt.description");

	public Type InputType => typeof(NoneInputState);

	public Type StateType => typeof(NoneInputState);

	public Task ExecuteAsync(
		Guid jobId,
		Guid leaseHolderId,
		CancellationToken cancellationToken = default)
		=> Task.CompletedTask;

	public string QuotaSystemName => QuotaName;
}
