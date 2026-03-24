using Microsoft.EntityFrameworkCore;

namespace Test.Common.Extensions;

public static class DbContextExtensions
{
    public static async Task ClearDatabase(this DbContext context)
    {
        var sql = """
                  DO $$
                  DECLARE
                      r RECORD;
                  BEGIN
                      FOR r IN 
                          SELECT schemaname, tablename
                          FROM pg_tables
                          WHERE schemaname IN ('auth', 'public')
                      LOOP
                          EXECUTE format('TRUNCATE TABLE %I.%I RESTART IDENTITY CASCADE', r.schemaname, r.tablename);
                      END LOOP;
                  END $$;
                  """;
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}