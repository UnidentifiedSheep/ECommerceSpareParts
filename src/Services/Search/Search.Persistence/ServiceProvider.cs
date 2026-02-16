using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Microsoft.Extensions.DependencyInjection;
using Search.Abstractions.Interfaces;
using Search.Abstractions.Interfaces.IndexDirectory;
using Search.Abstractions.Interfaces.Persistence;
using Search.Enums;
using Search.Persistence.Providers;
using Search.Persistence.Repositories;

namespace Search.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services, string indexDirectory)
    {
        services.AddSingleton<IIndexDirectory, IndexDirectory>(_ => new IndexDirectory(indexDirectory));
        services.AddSingleton<IIndexDirectoryProvider, IndexDirectoryProvider>();
        services.AddSingleton<StandardAnalyzer>(_ => new StandardAnalyzer(Global.LuceneVersion));

        services.AddSingleton<IArticleRepository, ArticleRepositoryBase>();
        services.AddSingleton<IArticleSuggestionRepository, ArticleSuggestionRepository>();
        
        return services;
    }
}