using Domain.Interfaces.Events;
using Enums;
using Pricing.Abstractions.Models;

namespace Pricing.Abstractions.Events;

public sealed record SupplierOffersRefreshedEvent(
	Supplier Supplier,
	string StorageCode,
	DateTime SeenAt,
	IReadOnlyList<SupplierOfferSnapshot> Offers) : IBatchableDomainEvent;
