using IntervalMap.Models;

namespace IntervalMap;

public abstract class IntervalMap<T>
{
    public abstract double MaxValue { get; protected set; }
    protected const int ScaleFactor = 100; //Множитель для конвертации double с двумя числами после запятой в int. 
    protected readonly List<T> Intervals = []; //Список доступных интервалов.
    /// <summary>
    /// Проверка на то, что числа имеют больше двух знаков после запятой или меньше.
    /// </summary>
    /// <param name="start">Начало интервала</param>
    /// <param name="end">Конец интервала</param>
    /// <returns>True если у числа меньше или ровно 2 знака после запятой, в ином случае false</returns>
    protected static bool CheckAfterComa(double start, double end) => CheckAfterComa(start) && CheckAfterComa(end);
    /// <summary>
    /// Проверка на то, что число имеет больше двух знаков после запятой или меньше.
    /// </summary>
    /// <param name="value">Число для проверки</param>
    /// <returns>True если у числа меньше или ровно 2 знака после запятой, в ином случае false </returns>
    protected static bool CheckAfterComa(double value) => (value * 100) % 1 == 0;
    protected static int Scale(double value) => (int)(value * ScaleFactor);

    /// <summary>
    /// Добавить интервал в карту.
    /// </summary>
    /// <param name="interval"></param>
    public abstract IntervalMap<T> AddInterval(T interval);

    /// <summary>
    /// Проверяет, содержит ли карта указанное значение.
    /// </summary>
    /// <param name="value">Число для проверки.</param>
    /// <returns>True, если значение принадлежит какому-либо интервалу, иначе False.</returns>
    public abstract bool Contains(double value);

    /// <summary>
    /// Возвращает интервал, которому принадлежит указанное значение.
    /// </summary>
    /// <param name="value">Число для поиска.</param>
    /// <returns>Интервал или null, если интервал не найден.</returns>
    public abstract T? GetInterval(double value);
    /// <summary>
    /// Удаление интервала с карты.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns>True если удалось удалить интервал и False если не удалось.</returns>
    public abstract bool RemoveInterval(T interval);
    /// <summary>
    /// Проверка на то существует ли пересечения с другими интервалами.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns>True если пересечение с уже существующим интервалом есть, в ином случае False.</returns>
    public abstract bool IsIntersectionExists(T interval);
}

public abstract class IntervalMap : IntervalMap<Interval>;