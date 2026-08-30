using HotChocolate;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Localization.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Tests.Tests.GraphQl.ErrorFilters;

internal static class ErrorFilterTestFactory
{
	public static IContextualStringLocalizer CreateLocalizer()
	{
		var container = new LocalizerContainer(new Locale("en"));
		container.Initialize(
			new Dictionary<string, string>
			{
				["Validation.Required"] = "Localized validation for {0}",
				["Db.Duplicate"] = "Duplicate {0}",
				["Domain.NotFound"] = "Entity {0} was not found"
			});

		return new ContextualStringLocalizer(
			new StringLocalizer([container]),
			Options.Create(
				new LocalesOptions
				{
					Default = "en", Supported = ["en"]
				}));
	}

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
			.SetPath(HotChocolate.Path.FromList(["field"]))
			.AddLocation(new Location(2, 3))
			.Build();
	}
}
