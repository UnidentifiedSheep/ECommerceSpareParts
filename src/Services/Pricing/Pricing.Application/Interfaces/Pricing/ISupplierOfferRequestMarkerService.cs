using Enums;

namespace Pricing.Application.Interfaces.Pricing;

public interface ISupplierOfferRequestMarkerService
{
    Task<bool> HasAnyMarkerAsync(
        Supplier supplier,
        int productId,
        string storageCode,
        CancellationToken token = default);

    Task MarkAsOkAsync(
        Supplier supplier,
        int productId,
        string storageCode,
        CancellationToken token = default);

    Task MarkAsOkAsync(
        IEnumerable<int> productId,
        Supplier supplier,
        string storageCode,
        CancellationToken token);

    Task MarkAsFailedAsync(
        Supplier supplier,
        int productId,
        string storageCode);
}