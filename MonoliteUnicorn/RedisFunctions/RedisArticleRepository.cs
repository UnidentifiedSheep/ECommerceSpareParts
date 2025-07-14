using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.StaticFunctions;
using Mapster;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.RedisFunctions.Models;
using StackExchange.Redis;

namespace MonoliteUnicorn.RedisFunctions;

public class RedisArticleRepository : IRedisArticleRepository
{
    private readonly IDatabase _redis;
    private readonly TimeSpan? _ttl;
    private LoadedLuaScript? _cachedScript;
    
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public RedisArticleRepository(IDatabase redis, TimeSpan? ttl = null)
    {
        _redis = redis;
        _ttl = ttl;
    }

    private static string GetArticleKey(int id) => $"article:{id}";
    private static string GetCrossesKey(int id) => $"article-crosses:{id}";
    private static string GetSortedCrossesKey(string field, int id) => $"article-crosses-sorted-by-{field}:{id}";
    private static string GetSortVariationsKey(int id) => $"article-crosses-sort-variations-{id}";
    private static string GetTrackingKey(int id) => $"article-used-in:{id}";
    
    public async Task SetArticles(IEnumerable<Article> articles)
    {
        var adaptedArticles = articles.Adapt<List<Versioned<ArticleFullDto>>>();
        var batch = _redis.CreateBatch();
        var tasks = new List<Task>();
        foreach (var article in adaptedArticles)
        {
            var key = GetArticleKey(article.Value.Id);
            var value = JsonSerializer.Serialize(article, JsonOptions);
            tasks.Add(batch.StringSetAsync(key, value, _ttl));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    private const string LuaSource = """
                                     local new = cjson.decode(@argv1)
                                     local ttl = tonumber(@argv2)
                                     local variations = cjson.decode(@argv3)
                                     local ranks = cjson.decode(@argv4)
                                     local usage = cjson.decode(@argv5)  -- список articleIds, где articleId используется
                                     
                                     local function update_article(articleId)
                                         local key = "article:" .. articleId
                                         local currentStr = redis.call('GET', key)
                                         local shouldSet = true
                                         if currentStr then
                                             local parsed = cjson.decode(currentStr)
                                             if new.version <= parsed.version then
                                                 shouldSet = false
                                             end
                                         end
                                         if shouldSet then
                                             redis.call('SET', key, @argv1, 'EX', ttl)
                                         end
                                         for i = 1, #variations do
                                             local field = variations[i]
                                             local sortKey = "article-crosses-sorted-by-" .. field .. ":" .. articleId
                                             local rank = ranks[field]
                                             if rank then
                                                 redis.call('ZADD', sortKey, rank, articleId)
                                                 redis.call('EXPIRE', sortKey, ttl)
                                             end
                                         end
                                     end
                                     
                                     update_article(new.value.id)
                                     
                                     for i = 1, #usage do
                                         local usedArticleId = usage[i]
                                         update_article(usedArticleId)
                                     end
                                     
                                     return 1
                                     """;


    public async Task UpdateArticlesWhichContains(IEnumerable<Article> articles)
    {
        var adaptedArticles = articles.Adapt<List<Versioned<ArticleFullDto>>>();
        if (adaptedArticles.Count == 0) return;

        if (_cachedScript == null)
        {
            var redisMux = (ConnectionMultiplexer)_redis.Multiplexer;
            var server = redisMux.GetEndPoints()
                .Select(endpoint => redisMux.GetServer(endpoint))
                .FirstOrDefault(s => !s.IsReplica && s.IsConnected)
                ?? throw new InvalidOperationException("No suitable Redis server found");

            var prepared = LuaScript.Prepare(LuaSource);
            _cachedScript = await prepared.LoadAsync(server);
        }

        var tasks = new List<Task<RedisResult>>();

        foreach (var article in adaptedArticles)
        {
            int articleId = article.Value.Id;
            string key = GetArticleKey(articleId);
            string json = JsonSerializer.Serialize(article, JsonOptions);
            int ttlSeconds = (int)(_ttl?.TotalSeconds ?? 0);

            var sortVariationEntries = await _redis.SetMembersAsync(GetSortVariationsKey(articleId));
            var sortVariations = sortVariationEntries.Select(x => x.ToString()).ToList();
            var variationsJson = JsonSerializer.Serialize(sortVariations, JsonOptions);

            var usageEntries = await _redis.SetMembersAsync(GetTrackingKey(articleId));
            var usage = usageEntries.Select(x => (int)x).ToList();

            var ranksFiltered = GetRanksOfArticle(article.Value, sortVariations);
            var ranksJson = JsonSerializer.Serialize(ranksFiltered, JsonOptions);

            var usageJson = JsonSerializer.Serialize(usage, JsonOptions);

            var task = _cachedScript.EvaluateAsync(_redis, new
            {
                key,
                argv1 = json,
                argv2 = ttlSeconds,
                argv3 = variationsJson,
                argv4 = ranksJson,
                argv5 = usageJson
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    public async Task<Dictionary<int, ArticleFullDto?>> GetArticles(IEnumerable<int> articleIds)
    {
        var ids = articleIds.Distinct().ToList();
        var keys = ids.Select(x => new RedisKey(GetArticleKey(x))).ToArray();
        var redisValues = await _redis.StringGetAsync(keys);
        var result = new Dictionary<int, ArticleFullDto?>();
        for (int i = 0; i < ids.Count; i++)
        {
            var id = ids[i];
            var value = redisValues[i];
            if (!value.HasValue || string.IsNullOrEmpty(value))
            {
                result.Add(id, null);
                continue;
            }
            var versioned = JsonSerializer.Deserialize<Versioned<ArticleFullDto>>(value!, JsonOptions);
            result.Add(id, versioned?.Value);
        }
        return result;
    }

    public async Task AddArticleCrosses(int articleId, IEnumerable<int> crosses)
    {
        var key = GetCrossesKey(articleId);
        var crossIds = crosses.Distinct()
            .Select(x => new SortedSetEntry(x, x))
            .ToArray();
        var whereUsed = crossIds.Select(x => GetTrackingKey((int)x.Element)).ToList();
        var batch = _redis.CreateBatch();
        var tasks = new List<Task>
        {
            batch.SortedSetAddAsync(key, crossIds),
            batch.KeyExpireAsync(key, _ttl),
            batch.SetAddAsync(GetTrackingKey(articleId), crossIds.Select(x => x.Element).ToArray()),
        };
        tasks.AddRange(whereUsed.Select(usage => batch.SetAddAsync(usage, articleId)));
        tasks.AddRange(whereUsed.Select(usage => batch.KeyExpireAsync(usage, _ttl)));
        batch.Execute();
        await Task.WhenAll(tasks);
    }

    public async Task<HashSet<int>> GetArticleCrosses(int articleId, int offset = 0, int limit = 100)
    {
        var key = GetCrossesKey(articleId);
        var crossIds = await _redis.SortedSetRangeByRankAsync(key, offset, offset+limit-1);
        return crossIds
            .Where(x => x.HasValue)
            .Select(x => int.Parse(x!))
            .ToHashSet();
    }

    public async Task RemoveArticleCrosses(int articleId, IEnumerable<int> crossIds)
    {
        var key = GetCrossesKey(articleId);
        var toRemove = crossIds.Distinct()
            .Select(x => new RedisValue(x.ToString()))
            .ToArray();
        await _redis.SortedSetRemoveAsync(key, toRemove);
    }

    public async Task ClearUsableArticleData(int articleId)
    {
        var keys = new List<RedisKey>
        {
            GetCrossesKey(articleId),
            GetArticleKey(articleId),
            GetSortVariationsKey(articleId)
        };

        var sortedCrossesKeys = (await _redis.SetMembersAsync(GetSortVariationsKey(articleId)))
            .Select(x => new RedisKey(GetSortedCrossesKey(x.ToString(), articleId)));

        keys.AddRange(sortedCrossesKeys);
        await _redis.KeyDeleteAsync(keys.ToArray());
    }

    public async Task AddArticleCrossesWithSort(int articleId, SortedSetEntry[] entries, 
        Expression<Func<ArticleFullDto, object>> sortFieldSelector)
    {
        var fieldName = Selector.GetFieldName(sortFieldSelector);
        var key = GetSortedCrossesKey(fieldName, articleId);
        var sortVariationsKey = GetSortVariationsKey(articleId);
        var trackingKey = GetTrackingKey(articleId);
        var batch = _redis.CreateBatch();
        entries = entries.Where(x => x.Element != articleId).ToArray();
        var articleIds = entries.Select(x => x.Element).ToArray();
        var tasks = new List<Task>
        {
            batch.SortedSetAddAsync(key, entries),
            batch.KeyExpireAsync(key, _ttl),
            batch.SetAddAsync(sortVariationsKey, fieldName),
            batch.KeyExpireAsync(sortVariationsKey, _ttl),
            batch.SetAddAsync(trackingKey, articleIds),
            batch.KeyExpireAsync(trackingKey, _ttl)
        };
        
        batch.Execute();
        await Task.WhenAll(tasks);
    }

    public async Task<HashSet<int>> GetArticleCrossesWithSort(int articleId, Expression<Func<ArticleFullDto, object>> sortFieldSelector, 
        int offset = 0, int limit = 100, string sort = "desc")
    {
        var fieldName = Selector.GetFieldName(sortFieldSelector);
        var key = GetSortedCrossesKey(fieldName, articleId);
        var order = sort.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? Order.Descending : Order.Ascending;
        var crossIds = await _redis.SortedSetRangeByRankAsync(key, offset, offset+limit-1, order);
        return crossIds
            .Where(x => x.HasValue)
            .Select(x => int.Parse(x!))
            .ToHashSet();
    }

    public async Task RemoveArticleCrossesWithSort(int articleId, IEnumerable<int> crossIds, Expression<Func<ArticleFullDto, object>> sortFieldSelector)
    {
        var fieldName = Selector.GetFieldName(sortFieldSelector);
        var key = GetSortedCrossesKey(fieldName, articleId);
        var toRemove = crossIds.Distinct()
            .Select(x => new RedisValue(x.ToString()))
            .ToArray();
        await _redis.SortedSetRemoveAsync(key, toRemove);
    }
    
    private async Task RemoveArticleEverywhere(int articleId)
    {
        var usedInKey = GetTrackingKey(articleId);
        var usedInArticles = await _redis.SetMembersAsync(usedInKey);

        var toRemoveValue = new RedisValue(articleId.ToString());

        var batch = _redis.CreateBatch();
        var tasks = new List<Task>();

        foreach (var usedInArticle in usedInArticles)
        {
            if (!int.TryParse(usedInArticle.ToString(), out var usedInId)) continue;
            
            var normalKey = GetCrossesKey(usedInId);
            tasks.Add(batch.SortedSetRemoveAsync(normalKey, toRemoveValue));
            
            var variationsKey = GetSortVariationsKey(usedInId);
            var fields = await _redis.SetMembersAsync(variationsKey);
            foreach (var field in fields)
            {
                var sortedKey = GetSortedCrossesKey(field.ToString(), usedInId);
                tasks.Add(batch.SortedSetRemoveAsync(sortedKey, toRemoveValue));
            }
            tasks.Add(batch.SetRemoveAsync(GetTrackingKey(usedInId), articleId));
        }
        
        tasks.Add(batch.KeyDeleteAsync(usedInKey));

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    private Dictionary<string, double> GetRanksOfArticle(ArticleFullDto article, IEnumerable<string> variations)
    {
        var variationsDistinct = variations.ToHashSet();
        var res = new Dictionary<string, double>();
        foreach (var field in variationsDistinct)
        {
            switch (field)
            {
                case nameof(ArticleFullDto.CurrentStock):
                    res[field] = article.CurrentStock;
                    break;
            }
        }
        return res;
    }
}