namespace Abstractions.Models;

public class UserEmailOptions
{
    /// <summary>Максимальное количество email-адресов на одного пользователя.</summary>
    public int MaxEmailCount { get; set; } = 255;

    public int MinEmailCount { get; set; } = 1;
}