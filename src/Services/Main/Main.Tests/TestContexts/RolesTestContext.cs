using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders.Auth;

namespace Tests.TestContexts;

public class RolesTestContext(DContext ctx, IMediator mediator) : TestContextBase<DContext>(ctx, mediator)
{
    public IReadOnlyCollection<Main.Entities.Auth.Role> Roles { get; private set; } = null!;
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var builders = new List<IBuilder<Main.Entities.Auth.Role>>();

        foreach (var value in Enum.GetValues<Role>())
            builders.Add(new RoleBuilder(Faker).WithName(value.ToString()));

        Roles = await BuilderExtensions.BuildManyCombinedAndAddToDb(
            DbContext,
            1,
            builders.ToArray());
    }
}