using System.Diagnostics.CodeAnalysis;
using HotChocolate;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Microsoft.AspNetCore.Http;
using Path = HotChocolate.Path;

namespace Tests.Tests.GraphQl.ErrorFilters;

internal static class ErrorFilterTestFactory
{
	public static IContextualLocalizer CreateLocalizer() => new TestContextualLocalizer(
		new Dictionary<string, string>
		{
			["Validation.Required"] = "Localized validation for Name",
			["Domain.NotFound"] = "Entity 42 was not found"
		});

	public static IHttpContextAccessor CreateHttpContextAccessor()
	{
		return new HttpContextAccessor
		{
			HttpContext = new DefaultHttpContext
			{
				TraceIdentifier = "test-trace-id"
			}
		};
	}

	public static IError CreateError(Exception exception)
	{
		return ErrorBuilder
			.New()
			.SetMessage("Unexpected Execution Error")
			.SetException(exception)
			.SetPath(Path.FromList(["field"]))
			.AddLocation(new Location(2, 3))
			.Build();
	}

	private sealed class TestContextualLocalizer(IReadOnlyDictionary<string, string> messages)
		: IContextualLocalizer
	{
		public string Get(ILocalizableMessage message) =>
			TryGet(message, out var value) ? value : throw new InvalidOperationException();

		public bool TryGet(ILocalizableMessage message, [NotNullWhen(true)] out string? value) =>
			messages.TryGetValue(message.MessageKey, out value);
	}
}
