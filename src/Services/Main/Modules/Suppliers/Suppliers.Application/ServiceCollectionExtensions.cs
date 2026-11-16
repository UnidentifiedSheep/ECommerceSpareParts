using Application.Common.Host;
using Application.Common.NamedObject;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Suppliers.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSuppliersApplication(this IServiceCollection services)
	{
		var assembly = SuppliersApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<SettingDefinitionNamedObjectBase>(assembly);

		return services;
	}
}
