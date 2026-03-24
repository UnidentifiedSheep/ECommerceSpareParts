using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Integration.Tests.MockData;

public static class DContextMockData
{
    public static async Task AddMockCurrencies(this DContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO currency (id, to_usd)
            VALUES 
                (1, 1),
                (2, 41.145504),
                (3, 80.400696),
                (4, 0.85493)
            ON CONFLICT (id) DO NOTHING;
            """);
    }
}