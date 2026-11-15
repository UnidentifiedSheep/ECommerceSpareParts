using Application.Common.Host;
using Application.Common.Interfaces.Lrt;
using Application.Common.NamedObject;
using Main.Application.Interfaces.Services;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Application.Services.ProductDeletion;
using Main.Application.Services.Storage;
using Main.Modules.Storage.Abstractions;
using Main.Modules.Product.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Storage.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStorageApplication(this IServiceCollection services)
	{
		var assembly = StorageApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly)
			.RegisterNamedObject<StorageContentExtractPolicyBase>(objectsLifetime: ServiceLifetime.Singleton)
			.RegisterNamedObject<SettingDefinitionNamedObjectBase>(assembly);

		services.AddScoped<IStorageContentService, StorageContentService>();
		services.AddScoped<IStorageChecks, StorageChecks>();
		services.AddScoped<IStorageContentSnapshotReader, StorageContentSnapshotReader>();
		services.AddScoped<IProductDeletionRule, StorageContentProductDeletionRule>();

		return services;
	}
}
