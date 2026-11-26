using Enums;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Application.Common.Models;
using Application.Common.Querying;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Logistics.Abstractions.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Commands.Pricing;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;

namespace Pricing.Application.Queries.Pricing;

public record GetPriceOptionsForProductQuery(
	int ProductId,
	int CurrencyId,
	string StorageCode,
	IReadOnlyCollection<PriceOfferSource> Sources,
	Pagination? Pagination,
	IReadOnlyCollection<SortExpression> SortBy) : IQuery<GetPriceOptionsForProductResult>;

public record GetPriceOptionsForProductResult(IReadOnlyCollection<PriceOptionDto> PriceOptions);

public class GetPriceOptionsForProductHandler(
	ISender sender,
	IReadRepository<ProductPriceOption, Guid> repository,
	ICurrencyConverter currencyConverter,
	IMarkupContainer markupContainer,
	IPriceApplierService priceApplierService,
	ISettingsService settingsService,
	IDeliveryEstimator deliveryEstimator)
	: IQueryHandler<GetPriceOptionsForProductQuery, GetPriceOptionsForProductResult>
{
	public async Task<GetPriceOptionsForProductResult> Handle(
		GetPriceOptionsForProductQuery request,
		CancellationToken cancellationToken)
	{
		var refreshed = await sender.Send(
			new RefreshOffersCommand(request.ProductId, request.StorageCode),
			cancellationToken);

		if (refreshed.CreatedOffers.Count != 0)
			await CalculateCandidates(request, cancellationToken);

		var options = await GetOptionsAsync(request, cancellationToken);
		var arrivalDates = await GetArrivalDatesAsync(options, cancellationToken);

		var result = new List<PriceOptionDto>();

		foreach (var option in options)
			result.Add(
				new PriceOptionDto
				{
					CurrencyId = request.CurrencyId,
					DeliveryProbability = option.DeliveryProbability,
					DeliveryDays = option.DeliveryDays,
					GuaranteedDeliveryDays = option.GuaranteedDeliveryDays,
					ForStorageCode = option.ForStorageCode,
					Markup = option.Markup,
					Price = await currencyConverter.ConvertAsync(
						option.Price,
						option.CurrencyId,
						request.CurrencyId,
						cancellationToken),
					Score = option.Score,
					PriceOfferId = option.PriceOfferId,
					PriceOffer = new PriceOfferDto
					{
						AvailableQuantity = option.PriceOffer.AvailableQuantity,
						CurrencyId = option.PriceOffer.CurrencyId,
						DaysToRefund = option.PriceOffer.DaysToRefund,
						DeliveryProbability = option.PriceOffer.DeliveryProbability,
						ExpiresAt = option.PriceOffer.ExpiresAt,
						DeliveryDate = GetArrivalDate(arrivalDates, option.PriceOffer, option.PriceOffer.DeliveryDays),
						GuaranteedDeliveryDate = GetArrivalDate(
							arrivalDates,
							option.PriceOffer,
							option.PriceOffer.GuaranteedDeliveryDays),
						OrderTill = option.PriceOffer.OrderTill,
						Id = option.PriceOffer.Id,
						MinimumPurchaseQuantity = option.PriceOffer.MinimumPurchaseQuantity,
						OfferForStorage = option.PriceOffer.OfferForStorage,
						ProductId = option.PriceOffer.ProductId,
						PurchasePrice = option.PriceOffer.PurchasePrice,
						Source = option.PriceOffer.Source,
						QuantityCoefficient = option.PriceOffer.QuantityCoefficient
					}
				});

		return new GetPriceOptionsForProductResult(result);
	}

	private async Task<Dictionary<(Supplier, string), IReadOnlyDictionary<int, DateTime>>> GetArrivalDatesAsync(
		IReadOnlyCollection<ProductPriceOption> options,
		CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;
		var result = new Dictionary<(Supplier, string), IReadOnlyDictionary<int, DateTime>>();

		var groups = options
			.Select(x => x.PriceOffer)
			.Where(x => x.Source.ToSupplier() != null)
			.GroupBy(x => (Supplier: x.Source.ToSupplier()!.Value, x.OfferForStorage));

		foreach (var group in groups)
			result[group.Key] = await deliveryEstimator.GetArrivalDatesAsync(
				group.Key.Supplier,
				group.Key.OfferForStorage,
				now,
				group
					.SelectMany(x => new[] { x.DeliveryDays, x.GuaranteedDeliveryDays })
					.Where(x => x.HasValue)
					.Select(x => x!.Value),
				cancellationToken);

		return result;
	}

	private static DateTime? GetArrivalDate(
		Dictionary<(Supplier, string), IReadOnlyDictionary<int, DateTime>> arrivalDates,
		PriceOffer offer,
		int? deliveryDays)
	{
		if (deliveryDays is not { } days || offer.Source.ToSupplier() is not { } supplier)
			return null;

		return arrivalDates.TryGetValue((supplier, offer.OfferForStorage), out var dates) &&
			dates.TryGetValue(days, out var date)
				? date
				: null;
	}

	private async Task CalculateCandidates(
		GetPriceOptionsForProductQuery request,
		CancellationToken cancellationToken)
		=> await sender.Send(
			new CalculateCandidatesCommand(request.ProductId, request.StorageCode),
			cancellationToken);

	private async Task<List<ProductPriceOption>> GetOptionsAsync(
		GetPriceOptionsForProductQuery request,
		CancellationToken cancellationToken)
	{
		var options = await GetOptionsCoreAsync(request, cancellationToken);
		var appliersVersion =
			await priceApplierService.GetCurrentConfigurationVersionAsync(cancellationToken);
		var pricingSettingsVersion = (await settingsService.GetOrDefault<PricingSetting>(cancellationToken))
			.Data.Version;

		if (options.Count == 0 || options.Any(x =>
				x.MarkupVersion != markupContainer.CurrentVersion ||
				x.AppliersVersion != appliersVersion ||
				x.PricingSettingsVersion != pricingSettingsVersion))
		{
			await CalculateCandidates(request, cancellationToken);
			options = await GetOptionsCoreAsync(request, cancellationToken);
		}

		return options;
	}

	private async Task<List<ProductPriceOption>> GetOptionsCoreAsync(
		GetPriceOptionsForProductQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository
			.Query
			.Include(x => x.PriceOffer)
			.Where(x => x.PriceOffer.ProductId == request.ProductId)
			.Where(x => x.PriceOffer.OfferForStorage == request.StorageCode)
			.Where(x => x.PriceOffer.ExpiresAt > DateTime.UtcNow);

		if (request.Sources.Count > 0)
			query = query.Where(x => request.Sources.Contains(x.PriceOffer.Source));

		return await query
			.SortBy(request.SortBy)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);
	}
}
