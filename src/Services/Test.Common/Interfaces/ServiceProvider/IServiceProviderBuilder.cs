using Test.Common.Interfaces.ServiceProvider;

namespace Test.Common.Interfaces;

public interface IServiceProviderBuilder<in TArgs> where TArgs : IServiceProviderArgument
{
    IServiceProvider Build(TArgs args);
}