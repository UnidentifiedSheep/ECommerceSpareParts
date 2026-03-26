using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.DependencyInjection;
using Search.Abstractions.Interfaces.Persistence;
using Search.Persistence.Analyzers;
using Search.Persistence.IndexContexts;
using Search.Persistence.Interfaces;
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

        //Analyzers
        services.AddSingleton<StandardAnalyzer>(_ => new StandardAnalyzer(Global.LuceneVersion));
        services.AddSingleton<RussianAnalyzer>(_ => new RussianAnalyzer(Global.LuceneVersion));
        services.AddSingleton<ArticleAnalyzer>(sp => new ArticleAnalyzer(sp.GetRequiredService<StandardAnalyzer>(),
            sp.GetRequiredService<RussianAnalyzer>()));

        services.AddSingleton<IIndexManager, IndexManager>();
        services.AddSingleton<IndexContext, ArticleIndexContext>();
        services.AddSingleton<IndexContext, ArticleSuggestionsContext>();

        services.AddSingleton<IArticleWriteRepository, ArticleWriteRepository>();
        services.AddSingleton<IArticleReadRepository, ArticleReadRepository>();
        services.AddSingleton<IArticleSuggestionRepository, ArticleSuggestionRepository>();

        services.AddSingleton<IArticleWriteService, ArticleWriteService>();
        services.AddSingleton<IArticleReadService, ArticleReadService>();
        services.AddSingleton<IArticleSuggestionService, ArticleSuggestionService>();

        return services;
    }
}