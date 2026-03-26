using Abstractions.Interfaces.Tests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Test.Common.Abstractions;

public abstract class TestContextBase<TDbContext>(TDbContext ctx, IMediator mediator)
    : ITestContext where TDbContext : DbContext
{
    public TDbContext DbContext => ctx;
    public IMediator Mediator => mediator;

    public abstract Task InitializeAsync(CancellationToken cancellationToken = default);
}