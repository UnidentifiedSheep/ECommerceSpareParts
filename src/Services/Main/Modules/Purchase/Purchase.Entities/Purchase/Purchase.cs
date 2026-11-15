using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Exceptions;
using Main.Enums;

namespace Main.Entities.Purchase;

public class Purchase : AuditableEntity<Purchase, Guid>,
	ILinqEntity<Purchase, Guid>,
	IGenerateAutomaticDomainEvents
{
	private readonly List<PurchaseContent> _contents = [];

	private Purchase()
	{
	}

	private Purchase(
		Guid supplierUserId,
		Guid supplierOrganizationId,
		int currencyId,
		Guid transactionId,
		string storage,
		DateTime purchaseDatetime)
	{
		Id = Guid.CreateVersion7();
		SupplierUserId = supplierUserId;
		SupplierOrganizationId = supplierOrganizationId;
		SetPurchaseDate(purchaseDatetime);
		SetCurrencyId(currencyId);
		TransactionId = transactionId;
		Storage = storage;
		State = PurchaseState.Draft;
	}

	[Validate]
	public Guid Id { get; }

	public Guid SupplierUserId { get; private set; }

	public Guid SupplierOrganizationId { get; private set; }

	public int CurrencyId { get; private set; }

	public Guid TransactionId { get; private set; }

	public string Storage { get; private set; } = null!;

	public DateTime PurchaseDatetime { get; private set; }

	public string? Comment { get; private set; }

	public PurchaseState State { get; private set; }

	public IReadOnlyCollection<PurchaseContent> Contents => _contents;

	public static Expression<Func<Purchase, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<Purchase, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;

	public static Purchase Create(
		Guid supplierUserId,
		Guid supplierOrganizationId,
		int currencyId,
		Guid transactionId,
		string storage,
		DateTime purchaseDatetime)
		=> new(
			supplierUserId,
			supplierOrganizationId,
			currencyId,
			transactionId,
			storage,
			purchaseDatetime);

	public void SetComment(string? comment)
		=> Comment = comment
			.NullIfWhiteSpace()
			?.EnsureMaxLength(
				256,
				() => throw new InvalidInputException(PurchaseCommentTooLongMessage.Instance));

	public void SetCurrencyId(int currencyId) => CurrencyId = currencyId;

	public void SetTransactionId(Guid transactionId) => TransactionId = transactionId;

	public void SetPurchaseDate(DateTime purchaseDatetime) => PurchaseDatetime = purchaseDatetime;

	public void AddContent(PurchaseContent content)
	{
		if (content.PurchaseId == Guid.Empty)
			content.SetPurchaseId(GetId());

		if (content.PurchaseId != GetId())
			throw new InvalidOperationException("Invalid purchase id in purchase content");
		if (_contents.Contains(content))
			return;
		_contents.Add(content);
	}

	public void RemoveContent(PurchaseContent content)
	{
		if (content.PurchaseId != GetId())
			throw new InvalidOperationException("Invalid purchase id in purchase content");

		_contents.Remove(content);
	}

	public void Complete() => State = PurchaseState.Completed;

	public override Guid GetId() => Id;
}
