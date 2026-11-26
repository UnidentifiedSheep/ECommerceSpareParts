using Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Pricing.Application.Commands.Pricing;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models;

namespace Tests.PricingTests.HandlerTests.Pricing;

public sealed class RefreshOffersHandlerTests
{
	[Fact]
	public async Task Handle_RequestCancelledAfterExtraction_StillAppliesOffersWithoutCancellation()
	{
		using var requestCancellation = new CancellationTokenSource();
		SupplierOfferExtractionResult[] extracted =
			[SupplierOfferExtractionResult.SupplierReturnedEmpty(Supplier.Armtek)];
		var extractor = new Mock<ISupplierOfferExtractorService>();
		extractor.Setup(x => x.ExtractOffers("main", 7, requestCancellation.Token))
			.ReturnsAsync(() =>
			{
				requestCancellation.Cancel();
				return extracted;
			});
		var sender = new Mock<ISender>();
		ApplyExtractedOffersCommand? sent = null;
		var sentToken = new CancellationToken(true);
		sender.Setup(x => x.Send(It.IsAny<ApplyExtractedOffersCommand>(), It.IsAny<CancellationToken>()))
			.Callback((IRequest<RefreshOffersResult> command, CancellationToken token) =>
			{
				sent = (ApplyExtractedOffersCommand)command;
				sentToken = token;
			})
			.ReturnsAsync(new RefreshOffersResult([]));

		await new RefreshOffersHandler(extractor.Object, sender.Object).Handle(
			new RefreshOffersCommand(7, "main"),
			requestCancellation.Token);

		sent.Should().NotBeNull();
		sent!.ProductId.Should().Be(7);
		sent.StorageCode.Should().Be("main");
		sent.Extracted.Should().BeSameAs(extracted);
		sentToken.CanBeCanceled.Should().BeFalse();
	}
}
