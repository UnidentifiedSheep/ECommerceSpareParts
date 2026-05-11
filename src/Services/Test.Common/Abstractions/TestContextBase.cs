using Bogus;
using Microsoft.EntityFrameworkCore;
using Test.Common.Interfaces;

namespace Test.Common.Abstractions;

public abstract class TestContextBase<TDbContext>(TDbContext ctx)
    : ITestContext where TDbContext : DbContext
{
    public TDbContext DbContext => ctx;
    public Faker Faker => new();

    public abstract Task InitializeAsync(CancellationToken cancellationToken = default);
}