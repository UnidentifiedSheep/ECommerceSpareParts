using Application.Common.Interfaces.Cqrs;
using MediatR;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities.Offers;

namespace Pricing.Application.Commands.Pricing;

public record RefreshOffersCommand(int ProductId, string StorageCode, bool ForceRefresh = false)
	: ICommand<RefreshOffersResult>;

public record RefreshOffersResult(IReadOnlyList<PriceOffer> CreatedOffers);

public class RefreshOffersHandler(ISupplierOfferExtractorService extractorService, ISender sender)
	: ICommandHandler<RefreshOffersCommand, RefreshOffersResult>
{
	public async Task<RefreshOffersResult> Handle(
		RefreshOffersCommand request,
		CancellationToken cancellationToken)
	{
		var extracted = await extractorService.ExtractOffers(
			request.StorageCode,
			request.ProductId,
			cancellationToken);

		return await sender.Send(
			new ApplyExtractedOffersCommand(
				DateTime.UtcNow,
				request.ProductId,
				request.StorageCode,
				extracted),
			CancellationToken.None);
	}
}
