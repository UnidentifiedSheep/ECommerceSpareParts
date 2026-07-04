using Abstractions.Interfaces.Mail;
using Abstractions.Models.Mail;
using Mailing.Core;
using RazorLight;

namespace EmailTemplates.Renderer;

public class EmailMessageRenderer(IRazorLightEngine engine) : IEmailMessageRenderer
{
    public async Task<IEmailMessage> RenderAsync<TTemplate>(
        TTemplate templateData,
        CancellationToken cancellationToken = default)
        where TTemplate : IEmailData
    {
        var body = await engine.CompileRenderAsync(
            $"{templateData.TemplateName}.cshtml",
            templateData);

        if (string.IsNullOrWhiteSpace(body))
            throw new InvalidOperationException(
                $"Email template '{templateData.TemplateName}' rendered empty body.");


        return new EmailMessage(
            templateData.Subject,
            templateData.To,
            body);
    }
}