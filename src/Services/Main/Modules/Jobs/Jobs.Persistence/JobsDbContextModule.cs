using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Jobs.Persistence;

public sealed class JobsDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder) => modelBuilder.ApplyJobConfigurations();
}
