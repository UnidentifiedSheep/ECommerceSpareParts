using Main.Entities.Auth;
using Main.Persistence.Context;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders.Auth;

namespace Tests.TestContexts;

public class RolesTestContext(DContext ctx) : TestContextBase<DContext>(ctx)
{
    public IReadOnlyCollection<Role> Roles { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var builders = new List<IBuilder<Role>>();

        foreach (var value in Enum.GetValues<Main.Enums.Role>())
            builders.Add(new RoleBuilder(Faker).WithName(value.ToString()));

        Roles = await BuilderExtensions.BuildManyCombinedAndAddToDb(
            DbContext,
            1,
            builders.ToArray());
    }
}