using MimeKit;

namespace Mail.Interfaces;

public interface IMail
{
    Task SendMailAsync(string receiver, string body = "", string subject = "", HeaderList? headers = null,
        CancellationToken cancellationToken = default);
}