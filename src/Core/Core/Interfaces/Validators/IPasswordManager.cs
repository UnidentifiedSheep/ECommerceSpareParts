namespace Core.Interfaces.Validators;

public interface IPasswordManager
{
    /// <summary>
    ///     Получить хеш пароля.
    /// </summary>
    string GetHashOfPassword(string password);

    /// <summary>
    ///     Проверить соответствие пароля и хеша.
    /// </summary>
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);

    /// <summary>
    ///     Проверка на соответствие правилам пароля.
    /// </summary>
    /// <param name="password">Пароль для проверки</param>
    /// <returns>isValid - соответствует ли правилам, errors - ошибки проверки</returns>
    (bool isValid, IEnumerable<string> errors) IsPasswordMatchRules(string password);
}