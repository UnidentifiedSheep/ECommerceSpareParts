using System.Runtime.CompilerServices;
using FluentAssertions;
using Main.Modules.Identity.Application.Notifications;
using RazorLight;

namespace Tests.Mailing;

public sealed class EmailTemplatesTests
{
	private static readonly string TemplatesRoot = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails");

	private static readonly RazorLightEngine Engine = new RazorLightEngineBuilder()
		.UseFileSystemProject(TemplatesRoot)
		.UseMemoryCachingProvider()
		.Build();

	public static TheoryData<string, Type> Templates => new()
	{
		{ "EmailVerification", typeof(EmailVerificationNotificationData) },
		{ "PasswordReset", typeof(PasswordResetNotificationData) },
		{ "UserLoggedIn", typeof(UserLoggedInNotificationData) }
	};

	[Fact]
	public void EveryTemplate_IsCovered()
	{
		var templates = Directory.EnumerateFiles(TemplatesRoot, "*.cshtml")
			.Select(Path.GetFileNameWithoutExtension);

		templates.Should().BeEquivalentTo(Templates.Select(row => row.Data.Item1));
	}

	[Theory]
	[MemberData(nameof(Templates))]
	public async Task Template_RendersWithItsModel(string systemName, Type modelType)
	{
		var model = RuntimeHelpers.GetUninitializedObject(modelType);

		var body = await Engine.CompileRenderAsync($"{systemName}.cshtml", model);

		body.Should().NotBeNullOrWhiteSpace();
	}
}
