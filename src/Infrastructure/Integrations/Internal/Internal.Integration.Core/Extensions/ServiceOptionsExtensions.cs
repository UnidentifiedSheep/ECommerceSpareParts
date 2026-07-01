using Abstractions;
using Abstractions.Interfaces;

namespace Internal.Integration.Core.Extensions;

public static class ServiceOptionsExtensions
{
    public static ServiceOptions GetOptionsForService(
        this InternalServicesOptions options,
        IServiceDefinition serviceDefinition)
    {
        return serviceDefinition switch
        {
            Main => options.Main,
            Analytics => options.Analytics,
            Pricing => options.Pricing,
            Search => options.Search,
            _ => throw new InvalidOperationException("This service definition is not supported.")
        };
    }
}