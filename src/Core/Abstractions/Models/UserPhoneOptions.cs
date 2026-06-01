namespace Abstractions.Models;

public class UserPhoneOptions
{
    public const string SectionName = "UserPhone";
    /// <summary>Максимальное количество номеров телефона на одного пользователя.</summary>
    public int MaxPhoneCount { get; set; } = 255;

    public int MinPhoneCount { get; set; } = 1;
}