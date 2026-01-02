using System.Linq.Expressions;

namespace Main.Core.Models;

public class ExistenceCheck
{
    public Type EntityType { get; }
    public Type KeyType { get; }
    public Type? ErrorType { get; }
    public HashSet<object> Keys { get; }
    public LambdaExpression KeySelector { get; }
    /// <summary>
    /// Если <c>true</c> то проверка на, то существуют ли. Успешно если все элементы существуют.
    /// Если <c>false</c> то проверка на, то не существуют ли. Успешно если все элементы отсутствуют.
    /// </summary>
    public bool Exists { get; }

    private ExistenceCheck(Type entityType, Type keyType, Type? errorType, IEnumerable<object> keys, bool exists, LambdaExpression keySelector)
    {
        EntityType = entityType;
        KeyType = keyType;
        Keys = new HashSet<object>(keys);
        KeySelector = keySelector;
        Exists = exists;
        ErrorType = errorType;
    }

    public object[] ValidateAndReturnMismatches(object[] foundValues)
    {
        var mismatches = new List<object>();
        var foundSet = new HashSet<object>(foundValues.Select(v => Convert.ChangeType(v, KeyType)));

        mismatches.AddRange(Exists
            ? Keys.Where(key => !foundSet.Contains(key))
            : foundSet.Where(found => Keys.Contains(found)));


        return mismatches.ToArray();
    }

    // Для одиночного ключа
    public static ExistenceCheck Create<TEntity, TKey>(TKey key, Expression<Func<TEntity, TKey>> keySelector, bool exists, Type? errorType = null)
    {
        if (errorType != null)
            ValidateErrorType(errorType);
        
        return new ExistenceCheck(typeof(TEntity), typeof(TKey), errorType, [key!], exists, keySelector);
    }

    // Для множества ключей
    public static ExistenceCheck CreateRange<TEntity, TKey>(IEnumerable<TKey> keys, Expression<Func<TEntity, TKey>> keySelector, bool exists, Type? errorType = null)
    {
        if (errorType != null)
            ValidateErrorType(errorType);
        return new ExistenceCheck(typeof(TEntity), typeof(TKey), errorType, keys.Cast<object>(), exists, keySelector);
    }

    private static void ValidateErrorType(Type type)
    {
        if (!typeof(Exception).IsAssignableFrom(type)) throw new ArgumentException($"Тип {type} не является наследником {nameof(Exception)}");
    }
}
