using Abstractions.Models;
using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Organizations;
using Main.Api.GraphQl.Types.Inputs.Organization;
using Main.Application.Handlers.Organizations.GetOrganizationMembers;
using Main.Application.Handlers.Balance.GetOrganizationFinancialInfo;
using Main.Application.Handlers.Balance.GetTransactions;
using MediatR;
using Main.Entities.Exceptions;
using Main.Enums.Organization;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Organization")]
public sealed record GqlOrganization
{
	private readonly OrganizationDto? _dto;

	public GqlOrganization(Guid id)
	{
		Id = id;
	}

	public GqlOrganization(OrganizationDto organization) : this(organization.Id)
	{
		_dto = organization;
	}

	[GraphQLName("id")]
	public Guid Id { get; }

	[GraphQLName("type")]
	public async Task<OrganizationType> GetTypeAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrganizationAsync(loader, cancellationToken)).Type;

	[GraphQLName("name")]
	public async Task<string> GetNameAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrganizationAsync(loader, cancellationToken)).Name;

	[GraphQLName("systemName")]
	public async Task<string> GetSystemNameAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrganizationAsync(loader, cancellationToken)).SystemName;

	[GraphQLName("isHidden")]
	public async Task<bool> GetIsHiddenAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrganizationAsync(loader, cancellationToken)).IsHidden;

	[GraphQLName("owner")]
	public async Task<GqlUser> GetOwnerAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		new((await GetOrganizationAsync(loader, cancellationToken)).Owner.User);

	[GraphQLName("approximateBalanceInBaseCurrency")]
	public async Task<decimal?> GetApproximateBalanceInBaseCurrencyAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		(await GetOrganizationAsync(loader, cancellationToken)).ApproximateBalanceInBaseCurrency;

	[GraphQLName("members")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_GET)]
	public async Task<IReadOnlyList<GqlOrganizationMember>> GetMembersAsync(
		GqlOrganizationMembersInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetOrganizationMembersQuery(Id, input.Pagination),
			cancellationToken);

		return result.Members.Select(member => new GqlOrganizationMember(member)).ToArray();
	}

	[GraphQLName("transactions")]
	[RequireAllPermissions(PermissionCodes.BALANCES_TRANSACTION_GET_ALL)]
	public async Task<IReadOnlyList<GqlTransaction>> GetTransactionsAsync(
		GqlOrganizationTransactionsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var cursor = input.Cursor.Value;
		var result = await sender.Send(
			new GetTransactionsQuery(
				new RangeModel<DateTime>(input.RangeStart, input.RangeEnd),
				input.CurrencyId,
				Id,
				Id,
				LogicalOperation.Or,
				new Cursor<(Guid id, DateTime dt)>(
					cursor is null ? (Guid.Empty, DateTime.MinValue) : (cursor.Value.Id, cursor.Value.TransactionDate),
					input.Cursor.Size),
				input.SkipReversed ?? false),
			cancellationToken);

		return result.Transactions.Select(transaction => new GqlTransaction(transaction)).ToArray();
	}

	[GraphQLName("finances")]
	[RequireAllPermissions(PermissionCodes.BALANCES_FINANCES_GET)]
	public async Task<GqlOrganizationFinances> GetFinancesAsync(
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetOrganizationFinancialInfoQuery(Id),
			cancellationToken);

		return new GqlOrganizationFinances(result.FinancialProfile, result.Balances);
	}

	private async Task<OrganizationDto> GetOrganizationAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(Id, cancellationToken)
		?? throw new OrganizationNotFoundException(Id);
}
