using Application.Common.Host;
using Main.Application.Interfaces.Services;
using Main.Application.Services;
using Main.Modules.Balance.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Balance.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddBalanceApplication(this IServiceCollection services)
	{
		services.AddModuleApplication(BalanceApplicationAssembly.Value);
		services.AddScoped<IBalanceService, BalanceService>();
		services.AddScoped<IFinanceTransfers, FinanceTransfers>();

		return services;
	}
}
