namespace Integrations.Supplier.Models;

public record DeliveryInfo
{
    public required DateTime DeliveryDate { get; init; }
    public required DateTime GuaranteedDeliveryDate { get; init; }
    public required int DeliveryProbability { get; init; }
    public required DateTime OrderTill { get; init; } //Dt till when the delivery dates are valid
}