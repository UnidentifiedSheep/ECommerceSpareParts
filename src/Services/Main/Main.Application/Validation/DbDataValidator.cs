using Exceptions.Base;
using Main.Core.Abstractions;
using Main.Core.Interfaces;
using Main.Core.Interfaces.Validation;
using Main.Core.Models;

namespace Main.Application.Validation;

public class DbDataValidator(ICombinedDataLoader combinedDataLoader) : DbDataValidatorBase
{
    public override async Task<Exception[]> Validate(IValidationPlan validationPlan, bool throwImid = true,
        bool throwCombined = true,
        CancellationToken cancellationToken = default)
    {
        var rules = validationPlan.Build();
        if (rules.Count == 0) return [];
        var result = await combinedDataLoader.GetExistenceChecks(rules, cancellationToken);
        var exceptions = new List<Exception>();
        foreach (var (rule, foundValues) in result)
        {
            var mismatches = rule.ValidateAndReturnMismatches(foundValues);
            if (mismatches.Length == 0) continue;
            var exception = GetNeededException(rule, mismatches);
            exceptions.Add(exception);
        }

        if (throwImid && exceptions.Count > 0)
        {
            if (throwCombined && exceptions.Count > 1)
                throw new GroupedException(exceptions);
            throw exceptions[0];
        }
        return exceptions.ToArray();
    }

    private static Exception GetNeededException(ExistenceCheck rule, object[] mismatches)
    {
        if (rule.ErrorType != null)
        {
            object ctorArg = mismatches.Length == 1 ? mismatches[0] : mismatches;
            if (typeof(BaseValuedException).IsAssignableFrom(rule.ErrorType))
                return (Activator.CreateInstance(rule.ErrorType, ctorArg) as BaseValuedException)!;
            return (Activator.CreateInstance(rule.ErrorType, ctorArg) as Exception)!;
        }
        

        if (rule.Exists)
            return new NotFoundException("Не удалось найти элемент/ы", mismatches);
        return new FoundException("Удалось найти элемент/ы", mismatches);
    }
}