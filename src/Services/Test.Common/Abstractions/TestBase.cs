using System.Reflection;
using Abstractions.Interfaces.Tests;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Interfaces;
using Xunit;

namespace Test.Common.Abstractions;

public abstract class TestBase : IAsyncLifetime, ITest
{
    private static readonly HashSet<Type> GlobalBasicContexts = [];
    private readonly HashSet<Type> _basicContexts = [];
    private readonly Dictionary<Type, ITestContext> _initedBasicContexts = new();
    private readonly HashSet<Type> _registeringContexts = [];

    protected readonly Faker Faker = new();
    protected abstract IServiceProvider Sp { get; }
    protected abstract IServiceScope Scope { get; }

    public abstract Task InitializeAsync();

    public abstract Task DisposeAsync();

    public void RegisterBasicContext<TContext>()
        where TContext : class, ITestContext
    {
        RegisterBasicContext(typeof(TContext));
    }

    public void RemoveBasicContext<TContext>() where TContext : class, ITestContext
    {
        _basicContexts.Remove(typeof(TContext));
    }

    private void RegisterBasicContext(Type type)
    {
        if (_basicContexts.Contains(type))
            return;

        if (!_registeringContexts.Add(type))
            throw new InvalidOperationException(
                $"Circular dependency detected for context {type.Name}");

        try
        {
            if (typeof(IDependentTestContext).IsAssignableFrom(type))
            {
                var dependsOnProperty = type.GetProperty(
                    nameof(IDependentTestContext.DependsOn),
                    BindingFlags.Public | BindingFlags.Static);

                if (dependsOnProperty == null)
                    throw new InvalidOperationException(
                        $"Type {type.Name} implements ITestContextRegistrator but does not define DependsOn.");

                var dependencies = dependsOnProperty.GetValue(null) as Type[]
                                   ?? [];

                foreach (var dependency in dependencies) RegisterBasicContext(dependency);
            }

            _basicContexts.Add(type);
        }
        finally
        {
            _registeringContexts.Remove(type);
        }
    }

    public static void RegisterGlobalBasicContext<TContext>() where TContext : class, ITestContext
    {
        GlobalBasicContexts.Add(typeof(TContext));
    }

    public static void RemoveGlobalBasicContext<TContext>() where TContext : class, ITestContext
    {
        GlobalBasicContexts.Remove(typeof(TContext));
    }

    protected T GetContext<T>() where T : class, ITestContext
    {
        if (_initedBasicContexts.TryGetValue(typeof(T), out var ctx))
            return (T)ctx;
        throw new InvalidOperationException($"No context found for {typeof(T).Name}. Try register it first");
    }

    protected virtual async Task InitializeBasicContexts()
    {
        var merged = GlobalBasicContexts.ToHashSet();
        merged.UnionWith(_basicContexts);
        foreach (var context in merged)
        {
            var ctx = (ITestContext)Scope.ServiceProvider.GetService(context)!;
            _initedBasicContexts.Add(ctx.GetType(), ctx);
            await ctx.InitializeAsync();
        }
    }
}