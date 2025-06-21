using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Core.StaticFunctions;

public static class EndpointConventionBuilderExtensions
{
    public static T WithGroup<T>(this T builder, string groupName) where T : IEndpointConventionBuilder
    {
        builder.WithMetadata(new ApiExplorerSettingsAttribute { GroupName = groupName });
        return builder;
    }
}