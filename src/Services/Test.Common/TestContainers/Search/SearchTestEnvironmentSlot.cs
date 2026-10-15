using Tests.TestContainers.OpenSearch;
using Tests.TestContainers.Redis;

namespace Tests.TestContainers.Search;

public sealed record SearchTestEnvironmentSlot(int Index, OpenSearchIndexSlot OpenSearch, RedisDatabaseSlot Redis);
