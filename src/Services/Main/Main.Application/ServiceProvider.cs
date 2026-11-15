using Application.Common.Services.Supplier;
using Abstractions;
using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Validators;
using Application.Common.Services.Currency;
using Application.Common.Validators;
using Main.Application.Configs;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.Currency;
using Main.Application.Interfaces.Services.Document;
using Main.Application.JobProviders;
using Main.Application.Lrts.ProducerImport;
using Main.Application.Models.Options;
using Main.Application.Products.Interfaces;
using Main.Application.Products.Services;
using Main.Application.Products.Services.EventParts;
using Main.Application.Services;
using Main.Application.Services.Currency;
using Main.Application.Services.ProductDeletion;
using Main.Modules.Product.Abstractions;
using Main.Application.Services.Storage;
using Main.Entities.Balance;
using Main.Modules.Organization.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Application;

public static class ServiceProvider
{
	public static IServiceCollection AddApplicationLayer(
		this IServiceCollection collection,
		IConfiguration? configuration)
	{
		var appLinksOptions = collection.AddOptions<AppLinksOptions>()
			.Validate(
				options => options.IsValid(),
				"App link templates must be relative and contain their required placeholders.")
			.ValidateOnStart();
		if (configuration is not null)
			appLinksOptions.Bind(configuration.GetSection(AppLinksOptions.SectionName));

		collection
			.AddNamedObjects()
			.AddLrtLayer(typeof(Global).Assembly)
			.RegisterJobProviders<DeleteProductImagesJobProvider>()
			.AddFusionCache()
			.WithRegisteredDistributedCache()
			.WithRegisteredBackplane()
			.WithSystemTextJsonSerializer();

		collection.RegisterProjectionProviders(typeof(Global).Assembly);

		collection.AddScoped<IAppLinkProvider, AppLinkProvider>();
		collection.AddScoped<IDocumentUrlProvider>(provider => provider.GetRequiredService<IAppLinkProvider>());
		collection.AddScoped<IAuthLinkProvider>(provider => provider.GetRequiredService<IAppLinkProvider>());

		collection.RegisterSettingsService();


		collection.AddScoped<ITransactionFinancialProfileService, TransactionFinancialProfileService>();

		collection.AddApplicationBase(
			ServicesDefinitions.Main,
			configuration,
			typeof(Global).Assembly);

		collection.AddScoped<ISupplierSettingSource, SupplierSettingSource>();

		SortByConfig.Configure();
		CursorConfig.Configure();

		return collection;
	}
}
