using HotChocolate;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.User;
using Main.Application.Dtos.Organizations;
using Main.Entities.Exceptions;
using Main.Enums.Organization;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Organization")]
public sealed record GqlOrganization
{
	private readonly OrganizationListItemDto? _dto;

	public GqlOrganization(Guid id)
	{
		Id = id;
	}

	public GqlOrganization(OrganizationListItemDto organization) : this(organization.Id)
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

	private async Task<OrganizationListItemDto> GetOrganizationAsync(
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken) =>
		_dto ?? await loader.LoadAsync(Id, cancellationToken)
		?? throw new OrganizationNotFoundException(Id);
}
