using Microsoft.EntityFrameworkCore;

namespace Tests.Extensions;

public static class DbContextExtensions
{
	public static async Task ClearDatabase(
		this DbContext context,
		CancellationToken cancellationToken = default)
	{
		var sql = """
				DO $$
				DECLARE
				    tables text;
				BEGIN
				    SELECT string_agg(format('%I.%I', schemaname, tablename), ', ')
				    INTO tables
				    FROM pg_tables
				    WHERE schemaname NOT IN ('pg_catalog', 'information_schema', 'materialized')
				      AND schemaname NOT LIKE 'pg\_%'
				      AND tablename <> '__EFMigrationsHistory';

				    IF tables IS NOT NULL THEN
				        EXECUTE 'TRUNCATE TABLE ' || tables || ' RESTART IDENTITY CASCADE';
				    END IF;
				END $$;
				""";
		await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
	}
}
