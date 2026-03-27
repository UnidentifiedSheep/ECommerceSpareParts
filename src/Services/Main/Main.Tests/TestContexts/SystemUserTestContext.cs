using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Tests.MockData.SeedExtensions;

namespace Tests.TestContexts;

public abstract class SystemUserTestContext(DContext context, IMediator mediator)
    : TestContextBase<DContext>(context, mediator)
{
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var systemUser = await DbContext.CreateSystemUser();
        Main.Application.Global.SetSystemId(systemUser.Id.ToString());
    }
}