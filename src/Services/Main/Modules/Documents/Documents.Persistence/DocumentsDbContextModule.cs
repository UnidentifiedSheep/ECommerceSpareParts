using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Documents.Persistence;

public sealed class DocumentsDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentsDbContextModule).Assembly);
}
