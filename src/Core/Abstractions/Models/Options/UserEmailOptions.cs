namespace Abstractions.Models.Options;

public class UserEmailOptions
{
    public const string SectionName = "UserEmail";

    /// <summary>Максимальное количество email-адресов на одного пользователя.</summary>
    public int MaxEmailCount { get; set; } = 255;

    public int MinEmailCount { get; set; } = 1;
}