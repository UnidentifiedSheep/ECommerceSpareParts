using Main.Entities.User;
using Main.Persistence.Context;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders.User;

namespace Tests.TestContexts;

public class UsersTestContext(
    DContext context
) : TestContextBase<DContext>(context)
{
    public IReadOnlyCollection<User> Users { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Users = await new MemberUserBuilder(Faker)
            .BuildManyAndAddToDb(DbContext, 3);
    }
}