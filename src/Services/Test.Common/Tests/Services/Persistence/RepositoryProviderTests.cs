using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Persistence;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.Tests.Services.Persistence;

public sealed class RepositoryProviderTests
{
    [Fact]
    public void Get_EntityAndKey_ReturnsWriteRepositoryFromCurrentScope()
    {
        var repository = Mock.Of<IRepository<TestEntity, int>>();
        using var scope = CreateProvider(services =>
                services.AddScoped(_ => repository))
            .CreateScope();
        var provider = new RepositoryProvider(scope.ServiceProvider);

        var result = provider.Get<TestEntity, int>();

        result.Should().BeSameAs(repository);
    }

    [Fact]
    public void GetForRead_EntityAndKey_ReturnsReadRepositoryFromCurrentScope()
    {
        var repository = Mock.Of<IReadRepository<TestEntity, int>>();
        using var scope = CreateProvider(services =>
                services.AddScoped(_ => repository))
            .CreateScope();
        var provider = new RepositoryProvider(scope.ServiceProvider);

        var result = provider.GetForRead<TestEntity, int>();

        result.Should().BeSameAs(repository);
    }

    [Fact]
    public void Get_SpecializedInterface_ReturnsRegisteredRepository()
    {
        var repository = Mock.Of<ITestRepository>();
        using var scope = CreateProvider(services =>
                services.AddScoped(_ => repository))
            .CreateScope();
        var provider = new RepositoryProvider(scope.ServiceProvider);

        var result = provider.Get<ITestRepository>();

        result.Should().BeSameAs(repository);
    }

    private static ServiceProvider CreateProvider(
        Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }

    public interface ITestRepository : IRepository<TestEntity, int>;

    public sealed class TestEntity : Entity<TestEntity, int>
    {
        public int Id { get; init; }
        public override int GetId() => Id;
    }
}
