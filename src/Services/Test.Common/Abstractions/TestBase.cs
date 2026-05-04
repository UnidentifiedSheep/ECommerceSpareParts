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
    protected abstract IServiceProvider Sp { get; }
    protected abstract IServiceScope Scope { get; }
    
    protected readonly Faker Faker = new();
    
    public void RegisterBasicContext<TContext>() 
        where TContext : class, ITestContext
    {
        var type = typeof(TContext);

        if (!_basicContexts.Add(type))
            return;

        if (!typeof(ITestContextRegistrator).IsAssignableFrom(type))
            return;

        var method = type.GetMethod(
            nameof(ITestContextRegistrator.Register),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(ITest)],
            modifiers: null) ?? throw new InvalidOperationException(
            $"Type {type.Name} implements ITestContextRegistrator but does not have correct static Register(ITest) method.");

        method.Invoke(null, [this]);
    }

    public void RemoveBasicContext<TContext>() where TContext : class, ITestContext
    {
        _basicContexts.Remove(typeof(TContext));
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

    protected async Task InitializeBasicContexts()
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

    public abstract Task InitializeAsync();

    public abstract Task DisposeAsync();
}