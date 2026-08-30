using System.Reflection;
using Analytics.Entities;
using Localization.Domain.Extensions;

namespace Analytics.Integration.Tests;

public class LocalizationTests : global::Tests.Tests.LocalizationTests
{
	[Theory]
	[InlineData("ru")]
	[InlineData("en")]
	[InlineData("tr")]
	public async Task All_LocalizableExceptions_Should_Have_Valid_Localization(string locale)
	{
		var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
		var assembly = Assembly.GetAssembly(typeof(PurchasesFact))!;

		await TestLocalizableExceptions(
			assembly,
			localesPath,
			locale);
	}
}
