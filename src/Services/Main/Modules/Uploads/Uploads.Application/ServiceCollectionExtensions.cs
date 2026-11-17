using Application.Common.Host;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Uploads.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddUploadsApplication(this IServiceCollection services)
		=> services.AddModuleApplication(UploadsApplicationAssembly.Value);
}
