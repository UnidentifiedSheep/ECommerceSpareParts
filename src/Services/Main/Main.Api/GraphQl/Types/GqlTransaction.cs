using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Balances;
using Main.Application.Handlers.Currencies;
using Main.Entities.Exceptions;
using Main.Enums.Balances;
using MediatR;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Transaction")]
public sealed record GqlTransaction
{
	private readonly TransactionDto? _dto;

	public GqlTransaction(Guid id)
	{
		Id = id;
	}

	public GqlTransaction(TransactionDto transaction) : this(transaction.Id)
	{
		_dto = transaction;
	}

	[GraphQLName("id")]
	public Guid Id { get; }

	[GraphQLName("amount")]
	public async Task<decimal> GetAmountAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetTransactionAsync(loader, cancellationToken)).Amount;

	[GraphQLName("transactionDate")]
	public async Task<DateTime> GetTransactionDateAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetTransactionAsync(loader, cancellationToken)).TransactionDate;

	[GraphQLName("type")]
	public async Task<TransactionType> GetTypeAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetTransactionAsync(loader, cancellationToken)).Type;

	[GraphQLName("sourceType")]
	public async Task<TransactionSourceType> GetSourceTypeAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetTransactionAsync(loader, cancellationToken)).SourceType;

	[GraphQLName("sender")]
	public async Task<GqlOrganization> GetSenderAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetTransactionAsync(loader, cancellationToken)).Sender);

	[GraphQLName("receiver")]
	public async Task<GqlOrganization> GetReceiverAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetTransactionAsync(loader, cancellationToken)).Receiver);

	[GraphQLName("currency")]
	public async Task<GqlCurrency> GetCurrencyAsync(
		ITransactionByIdDataLoader loader,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var transaction = await GetTransactionAsync(loader, cancellationToken);
		var result = await sender.Send(new GetCurrencyByIdQuery(transaction.CurrencyId), cancellationToken);
		return new GqlCurrency(result.Currency);
	}

	[GraphQLName("isCompleted")]
	public async Task<bool> GetIsCompletedAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetTransactionAsync(loader, cancellationToken)).Status.HasFlag(TransactionStatus.Completed);

	[GraphQLName("isReversed")]
	public async Task<bool> GetIsReversedAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetTransactionAsync(loader, cancellationToken)).Status.HasFlag(TransactionStatus.Reversed);

	[GraphQLName("reversedAt")]
	public async Task<DateTime?> GetReversedAtAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetTransactionAsync(loader, cancellationToken)).ReversedAt;

	[GraphQLName("reversedBy")]
	public async Task<GqlUser?> GetReversedByAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var userId = (await GetTransactionAsync(loader, cancellationToken)).ReversedBy;
		return userId.HasValue ? new GqlUser(userId.Value) : null;
	}

	private async Task<TransactionDto> GetTransactionAsync(
		ITransactionByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(Id, cancellationToken)
		?? throw new TransactionNotFoundException(Id);
}
