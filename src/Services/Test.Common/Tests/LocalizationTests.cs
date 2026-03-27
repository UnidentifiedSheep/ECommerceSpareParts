using System.Collections;
using System.Reflection;
using Abstractions.Interfaces.Exceptions;
using BulkValidation.Core.Configuration;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Models;
using FluentAssertions;
using FluentValidation;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Localization.Domain;
using Moq;

namespace Test.Common.Tests;

public class LocalizationTests
{
    public async Task TestLocalizableExceptions(Assembly exceptionsAssembly, string path, Locale locale)
    {
        using var scoped = await CreateLocalizer(path, locale);
        scoped.SetLocale(locale);

        var exceptionTypes = exceptionsAssembly.DefinedTypes
            .Where(t => typeof(ILocalizableException).IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false });

        foreach (var type in exceptionTypes)
        {
            var instances = CreateExceptionInstances(type);

            foreach (var ex in instances)
            {
                var exists = scoped.TryGet(ex.MessageKey, out var template);
                exists.Should().BeTrue($"Missing key '{ex.MessageKey}' in {type.Name}");

                template.Should().NotBeNull();

                var args = ex.Arguments ?? [];

                var act = () => { _ = string.Format(template, args); };
                act.Should().NotThrow($"Formatting failed for key '{ex.MessageKey}'");
            }
        }
    }

    public async Task TestAbstractValidatorLocalization(Assembly validatorsAssembly, string path, Locale locale)
    {
        using var scoped = await CreateLocalizer(path, locale);
        scoped.SetLocale(locale);

        var validatorTypes = validatorsAssembly.DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(IValidator<>)));

        foreach (var type in validatorTypes)
        {
            var validator = CreateValidator(type);

            var descriptor = validator.CreateDescriptor();
            var membersAndValidators = descriptor.GetMembersWithValidators()
                .SelectMany(x => x.ToList());


            foreach (var (v, o) in membersAndValidators)
            {
                var errorCode = o.ErrorCode;

                if (string.IsNullOrWhiteSpace(errorCode))
                    continue;

                var success = scoped.TryGet(errorCode, out _);

                success.Should().BeTrue($"Missing key '{errorCode}' in {type.Name}");
            }
        }
    }
    
    public async Task TestDbValidatorLocalization(IEnumerable<string> functionKeys, string path, Locale locale)
    {
        using var scoped = await CreateLocalizer(path, locale);
        scoped.SetLocale(locale);

        foreach (var key in functionKeys)
        {
            var configs = GetConfigs(key);
            if (configs.Count == 0) continue;

            foreach (var config in configs)
            {
                var messageKey = config.MessageTemplate;
                var success = scoped.TryGet(messageKey, out _);

                success.Should().BeTrue($"Missing key '{messageKey}' for function {key}");
            }
        }
    }

    private static List<ValidationConfig> GetConfigs(string key)
    {
        var res = new List<ValidationConfig?>()
        {
            ConfigureDbValidation.GetConfig(key, KeyValueType.Single),
            ConfigureDbValidation.GetConfig(key, KeyValueType.MultipleKeys),
            ConfigureDbValidation.GetConfig(key, KeyValueType.Tuple)
        };

        return res.Where(x => x != null).Select(x => x!).ToList();
    }

    private static IValidator CreateValidator(Type type)
    {
        var ctor = type.GetConstructors().First();

        var args = ctor.GetParameters()
            .Select(p => GetDefault(p.ParameterType))
            .ToArray();

        return (IValidator)Activator.CreateInstance(type, args)!;
    }

    private async Task<IScopedStringLocalizer> CreateLocalizer(string path, Locale locale)
    {
        var jsonLoader = new JsonLocalizerContainerLoader(path);
        var container = new LocalizerContainer(locale);

        await jsonLoader.LoadAsync([container]);

        var localizer = new StringLocalizer([container]);
        return new ScopedStringLocalizer(localizer);
    }

    private static IEnumerable<ILocalizableException> CreateExceptionInstances(Type type)
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

        if (type.IsValueType || (!type.IsInterface && !type.IsAbstract))
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        var mockType = typeof(Mock<>).MakeGenericType(type);
        var mock = Activator.CreateInstance(mockType);
        var objectProperty = mockType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .First(p => p.Name == "Object");
        return objectProperty.GetValue(mock);
    }
}