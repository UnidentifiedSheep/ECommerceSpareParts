using GraphQL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Security.Authorization;

namespace GraphQL.Common.Authorization;

internal sealed record GraphQlFieldAuthorizationRequirement(
	GraphQlAuthorizationTarget Target,
	AuthorizationMatch Match) : IAuthorizationRequirement;
