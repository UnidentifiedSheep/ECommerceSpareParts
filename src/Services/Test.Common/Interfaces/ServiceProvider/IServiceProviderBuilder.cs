namespace Tests.Interfaces.ServiceProvider;

public interface IServiceProviderBuilder<in TArgs> where TArgs : IServiceProviderArgument
{
    IServiceProvider Build(TArgs args);
}