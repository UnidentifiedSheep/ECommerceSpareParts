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

    public required SecureSocketOptions SecureSocket { get; set; } 
        = SecureSocketOptions.Auto;
}