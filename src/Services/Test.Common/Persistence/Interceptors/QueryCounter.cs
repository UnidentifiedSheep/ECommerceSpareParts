using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tests.Persistence.Interceptors;

public sealed class QueryCounter : DbCommandInterceptor
{
	public int Reads { get; private set; }

	public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
		DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
		CancellationToken cancellationToken = default)
	{
		Reads++;
		return ValueTask.FromResult(result);
	}
}
