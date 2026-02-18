using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.DependencyInjection;
using Search.Abstractions.Interfaces.Persistence;
using Search.Persistence.Interfaces.IndexDirectory;
using Search.Persistence.Interfaces.Repositories;
using Search.Persistence.Providers;
using Search.Persistence.Repositories;
using Search.Persistence.Services;

namespace Search.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services, string indexDirectory)
    {
        services.AddSingleton<IIndexDirectory, IndexDirectory>(_ => new IndexDirectory(indexDirectory));
        services.AddSingleton<IIndexDirectoryProvider, IndexDirectoryProvider>();
        services.AddSingleton<StandardAnalyzer>(_ => new StandardAnalyzer(Global.LuceneVersion));

        services.AddSingleton<IArticleRepository, ArticleRepository>();
        services.AddSingleton<IArticleSuggestionRepository, ArticleSuggestionRepository>();

        services.AddSingleton<IArticleService, ArticleService>();
        services.AddSingleton<IArticleSuggestionService, ArticleSuggestionService>();
        
        return services;
    }
}