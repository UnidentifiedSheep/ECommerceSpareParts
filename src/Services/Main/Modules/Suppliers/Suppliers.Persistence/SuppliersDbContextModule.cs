using Domain.CommonEntities;
using Main.Entities.Suppliers.Settings.Supplier;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Suppliers.Persistence;

public sealed class SuppliersDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<FavoritSupplierSetting>(FavoritSupplierSetting.SettingName)
			.HasValue<TmtrSupplierSetting>(TmtrSupplierSetting.SettingName)
			.HasValue<ArmtekSupplierSetting>(ArmtekSupplierSetting.SettingName);
}
