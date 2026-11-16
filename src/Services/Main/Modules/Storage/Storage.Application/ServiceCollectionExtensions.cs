using NamedObject.Core.Interfaces;
using Application.Common.Host;
using Application.Common.Interfaces;
using Application.Common.NamedObject;
using Main.Modules.Storage.Abstractions;
using Main.Modules.Product.Abstractions;
using Main.Modules.Storage.Abstractions.Interfaces;
using Main.Modules.Storage.Application.Configs;
using Main.Modules.Storage.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Modules.Storage.Application.Services.ProductDeletion;
using Main.Modules.Storage.Application.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;

namespace Main.Modules.Storage.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStorageApplication(this IServiceCollection services)
	{
		services.AddSingleton<INamedObjectGroupRegistry, StorageNamedObjectGroupRegistry>();

		services.AddValidationErrors(StorageValidationConfiguration.Configure);
		StorageSortByConfig.Configure();

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
