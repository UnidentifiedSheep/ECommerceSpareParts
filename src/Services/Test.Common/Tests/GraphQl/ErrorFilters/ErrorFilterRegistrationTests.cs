using Abstractions.Interfaces;
using FluentAssertions;
using GraphQL.Common.Extensions;
using HotChocolate.Execution;
using Localization.Abstractions.Models;
using Localization.Domain.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Tests.Stubs;

namespace Tests.Tests.GraphQl.ErrorFilters;

public class ErrorFilterRegistrationTests
{
	[Fact]
	public async Task AddCommonGraphQl_ShouldApplyRegisteredErrorFilters()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddSingleton<IUserContext, UserContextMock>();
		services.AddLocalization(new Locale("en"), new Locale("en"));
		services.AddCommonGraphQl("error-filter-tests").AddQueryType<ErrorFilterTestQuery>();
		await using var serviceProvider = services.BuildServiceProvider();
		var executor = await serviceProvider.GetRequestExecutorAsync("error-filter-tests");

		var result = await executor.ExecuteAsync("{ failure }");

		var error = result.ExpectOperationResult().Errors.Should().ContainSingle().Subject;
		error.Message.Should().Be("InternalServerException");
		error.Code.Should().Be("INTERNAL_SERVER_ERROR");
		error.Exception.Should().BeNull();
	}
}

public sealed class ErrorFilterTestQuery
{
	public string Failure() => throw new InvalidOperationException("sensitive message");
}
