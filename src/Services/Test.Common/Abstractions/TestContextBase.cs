using Bogus;
using Microsoft.EntityFrameworkCore;
using ITestContext = Tests.Interfaces.ITestContext;

namespace Tests.Abstractions;

public abstract class TestContextBase<TDbContext>(TDbContext ctx) : ITestContext where TDbContext : DbContext
{
	public TDbContext DbContext => ctx;

	public Faker Faker => new();

	public abstract Task InitializeAsync(CancellationToken cancellationToken = default);
}
