using Main.Entities.Balance;
using Application.Common.Interfaces.Lrt;
using NamedObject;
using Application.Common.Host;
using Application.Common.Interfaces;
using Main.Entities.Balance.Balance;
using Main.Modules.Balance.Abstractions;
using Main.Modules.Balance.Application.Configs;
using Main.Modules.Balance.Application.Interfaces;
using Main.Modules.Balance.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Balance.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddBalanceApplication(this IServiceCollection services)
	{
		services.AddValidationErrors(BalanceValidationConfiguration.Configure);
		services.AddScoped<ITransactionFinancialProfileService, TransactionFinancialProfileService>();

		services
			.AddModuleApplication(BalanceApplicationAssembly.Value)
			.RegisterNamedObject<ILrtNamedObject>(BalanceApplicationAssembly.Value);
		services.AddScoped<IBalanceService, BalanceService>();
		services.AddScoped<IFinanceTransfers, FinanceTransfers>();

		return services;
	}
}
