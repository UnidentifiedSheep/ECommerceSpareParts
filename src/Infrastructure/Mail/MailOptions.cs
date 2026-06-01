using System.ComponentModel.DataAnnotations;
using MailKit.Security;

namespace Mail;

public class MailOptions
{
    public const string SectionName = "Mail";
    
    [Required]
    public required string Host { get; set; }
    
    [Required]
    public required int Port { get; set; }
    
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
    
    [Required]
    public required string FromName { get; set; }
    
    [Required]
    public required string FromEmail { get; set; }

    public required int MaxBatchSize { get; set; } = 10;
    public required TimeSpan BatchDelay { get; set; } = TimeSpan.FromMilliseconds(300);
    public required int MaxRetryAttempts { get; set; } = 3;
    public required TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    public required SecureSocketOptions SecureSocket { get; set; } 
        = SecureSocketOptions.Auto;
}
