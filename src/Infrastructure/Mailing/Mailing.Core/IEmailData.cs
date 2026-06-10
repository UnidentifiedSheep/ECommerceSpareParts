namespace Mailing.Core;

public interface IEmailData
{
    string TemplateName { get; }
    string Subject { get; }
    string To { get; }
}