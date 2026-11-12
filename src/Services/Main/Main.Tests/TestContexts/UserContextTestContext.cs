using Application.Common.Interfaces.Settings;
using Persistence.Context;
using Security.Core.Interfaces;
using Tests.Stubs;

namespace Tests.TestContexts;

public class UserContextTestContext(
	UnifiedDbContext context,
	ISettingsService settingsService,
	TestSystemOptionsAccessor systemOptionsAccessor,
	IUserContext userContext) : GlobalApplicationSettingTestContext(
	context,
	settingsService,
	systemOptionsAccessor)
{
	public IUserContext UserContext { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		await base.InitializeAsync(cancellationToken);
		if (userContext is not UserContextMock uc)
			throw new InvalidOperationException(
				"IUserContext is not UserContextMock. For test it must be UserContextMock");

		UserContext = uc;

		uc.SetIsAuthenticated(true).SetUserId(SystemUser.Id);
	}
}
