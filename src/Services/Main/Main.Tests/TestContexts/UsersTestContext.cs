using Main.Entities.User;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders.User;

namespace Tests.TestContexts;

public class UsersTestContext(
    DContext context,
    IMediator mediator) : TestContextBase<DContext>(context, mediator)
{
    public IReadOnlyCollection<User> Users { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Users = await new MemberUserBuilder(Faker)
            .BuildManyAndAddToDb(DbContext, 3);
    }
}