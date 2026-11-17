using Microsoft.EntityFrameworkCore;

namespace Persistence.Interfaces;

public interface IDbContextModule
{
	void Configure(ModelBuilder modelBuilder);

	void PostConfigure(ModelBuilder modelBuilder) { }
}
