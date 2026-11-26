using Application.Common.Interfaces.Cqrs;
using Attributes;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;

namespace Pricing.Application.Commands.Pricing;

[Transactional]
[AutoSave]
public record ApplyExtractedOffersCommand(
	DateTime ExtractedAt,
	int ProductId,
	string StorageCode,
	IReadOnlyList<SupplierOfferExtractionResult> Extracted) : ICommand<RefreshOffersResult>;

public class ApplyExtractedOffersHandler(IOfferRefreshService refreshService)
	: ICommandHandler<ApplyExtractedOffersCommand, RefreshOffersResult>
{
	public async Task<RefreshOffersResult> Handle(
		ApplyExtractedOffersCommand request,
		CancellationToken cancellationToken)
	{
		var offers = await refreshService.ApplyExtractedOffersAsync(
			request.ExtractedAt,
			request.ProductId,
			request.StorageCode,
			request.Extracted,
			cancellationToken);

		return new RefreshOffersResult(offers);
	}
}
