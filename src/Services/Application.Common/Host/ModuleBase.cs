using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Host;

public abstract class ModuleBase
{
	public abstract void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
