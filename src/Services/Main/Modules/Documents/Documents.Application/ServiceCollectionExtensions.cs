using Main.Modules.Documents.Application.Configs;
using Application.Common.Extensions;
using Application.Common.Host;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Lrt;
using Main.Application.Document.Interfaces.Document;
using Main.Modules.Documents.Application.Interfaces;
using Main.Modules.Documents.Application.Models.Options;
using Main.Modules.Documents.Application.Notifications;
using Main.Modules.Documents.Application.Services;
using Main.Modules.Documents.Application.Services.Document;
using Main.Modules.Documents.Application.Services.Document.Providers;
using Main.Modules.Documents.Application.Services.Document.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NamedObject;
using Notification.Core;
using Scrutor;

namespace Main.Modules.Documents.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDocumentLinks(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<DocumentLinksOptions>()
			.Bind(configuration.GetSection(DocumentLinksOptions.SectionName))
			.Validate(
				options => options.IsValid(),
				"Document link template must be relative and contain its required placeholder.")
			.ValidateOnStart();
		services.AddScoped<IDocumentUrlProvider, DocumentUrlProvider>();

		return services;
	}

	public static IServiceCollection AddDocumentsApplication(this IServiceCollection services)
	{
		DocumentsSortByConfig.Configure();
		services.AddNotification<DocumentGeneratedNotification, DocumentGeneratedNotificationData>(
			DocumentGeneratedNotification.NotificationSystemName,
			model => new DocumentGeneratedNotification(model));
		services.RegisterNamedObject<IDocumentDefinition>(objectsLifetime: ServiceLifetime.Scoped);

		var assembly = DocumentsApplicationAssembly.Value;

		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly)
			.RegisterJobProviders(assembly);

		services.AddScoped<IDocumentLinkProvider, DocumentLinkProvider>();

		//order is important
		services.AddSingleton<IDocumentTemplateSource, S3DocumentTemplateSource>();
		services.AddSingleton<IDocumentTemplateSource, EmbeddedDocumentTemplateSource>();
		services.Decorate<IDocumentTemplateSource, CachedDocumentTemplateSource>();

		services.AddSingleton<IDocumentTemplateCache, DocumentTemplateCache>();
		services.AddSingleton<IDocumentProvider, ExcelDocumentProvider>();
		services.AddSingleton<IDocumentTemplateResolver, DocumentTemplateResolver>();

		return services;
	}
}
