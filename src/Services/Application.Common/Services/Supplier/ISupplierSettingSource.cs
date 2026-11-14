namespace Application.Common.Services.Supplier;

public interface ISupplierSettingSource
{
	Task<string?> GetSettingJsonAsync(string settingSystemName, CancellationToken cancellationToken = default);
}
