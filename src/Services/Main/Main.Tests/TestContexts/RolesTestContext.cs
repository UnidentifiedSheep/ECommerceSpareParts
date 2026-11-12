using Persistence.Context;
using Main.Entities.Auth;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Auth;
using Tests.Extensions;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class RolesTestContext(UnifiedDbContext ctx) : TestContextBase<UnifiedDbContext>(ctx)
{
	public IReadOnlyCollection<Role> Roles { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var builders = new List<IBuilder<Role>>();

		foreach (var value in Enum.GetValues<Enums.Role>())
			builders.Add(new RoleBuilder(Faker).WithName(value.ToString()));

		Roles = await BuilderExtensions.BuildManyCombinedAndAddToDb(
			DbContext,
			1,
			true,
			builders.ToArray());
	}
}
