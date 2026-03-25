using System.Collections;
using System.Reflection;
using Abstractions.Interfaces.Exceptions;
using FluentAssertions;
using Localization.Domain;
using Main.Abstractions.Utils;

namespace Tests.LocalizationTests;

public class ExceptionLocalizationTests
{
    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    public async Task All_LocalizableExceptions_Should_Have_Valid_Localization(string locale)
    {
        string localesPath = Assembly.GetExecutingAssembly().Location;
        localesPath = Path.Combine(Path.GetDirectoryName(localesPath)!, "Localization");
        var jsonLoader = new JsonLocalizerContainerLoader(localesPath);
        var container = new LocalizerContainer(locale);
        
        await jsonLoader.LoadAsync([container]);
        
        var localizer = new StringLocalizer([container]);
        var scoped = new ScopedStringLocalizer(localizer);
        scoped.SetLocale(locale);

        var exceptionTypes = Assembly.GetAssembly(typeof(CacheKeys))!.DefinedTypes
            .Where(t => typeof(ILocalizableException).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.IsInterface);

        foreach (var type in exceptionTypes)
        {
            var instances = CreateInstances(type);

            foreach (var ex in instances)
            {
                var exists = scoped.TryGet(ex.MessageKey, out var template);
                exists.Should().BeTrue($"Missing key '{ex.MessageKey}' in {type.Name}");

                template.Should().NotBeNull();

                var args = ex.Arguments ?? [];

                Action act = () => { _ = string.Format(template, args); };
                act.Should().NotThrow($"Formatting failed for key '{ex.MessageKey}'");
            }
        }
    }
    
    private static IEnumerable<ILocalizableException> CreateInstances(Type type)
    {
        var result = new List<ILocalizableException>();

        foreach (var ctor in type.GetConstructors())
        {
            var parameters = ctor.GetParameters();

            // генерим "фейковые" значения
            var args = parameters.Select(p => GetDefault(p.ParameterType)).ToArray();

            try
            {
                if (Activator.CreateInstance(type, args) is ILocalizableException ex)
                    result.Add(ex);
            }
            catch
            {
                //ignore
            }
        }

        return result;
    }

    private static object? GetDefault(Type type)
    {
        if (type == typeof(string)) return "test";
        if (type == typeof(Guid)) return Guid.NewGuid();
        if (type == typeof(int)) return 1;
        if (type == typeof(long)) return 1L;
        if (type == typeof(decimal)) return 1m;
        if (type == typeof(double)) return 1.0;
        if (type == typeof(bool)) return true;

        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var array = Array.CreateInstance(elementType, 1);
            array.SetValue(GetDefault(elementType), 0);
            return array;
        }

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            var argType = type.GetGenericArguments()[0];

            if (typeof(IEnumerable<>).IsAssignableFrom(genericType) ||
                typeof(ICollection<>).IsAssignableFrom(genericType) ||
                typeof(IList<>).IsAssignableFrom(genericType) ||
                genericType == typeof(List<>))
            {
                var listType = typeof(List<>).MakeGenericType(argType);
                var list = (IList)Activator.CreateInstance(listType)!;
                list.Add(GetDefault(argType));
                return list;
            }
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var args = type.GetGenericArguments();
            var dict = (IDictionary)Activator.CreateInstance(type)!;
            dict.Add(GetDefault(args[0])!, GetDefault(args[1])!);
            return dict;
        }

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}