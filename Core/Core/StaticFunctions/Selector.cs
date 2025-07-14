using System.Linq.Expressions;

namespace Core.StaticFunctions;

public static class Selector
{
    public static string GetFieldName<T>(Expression<Func<T, object>> selector)
    {
        return selector.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression unaryMember } => unaryMember.Member.Name,
            _ => throw new ArgumentException("Invalid selector. Must be a member expression.")
        };
    }
}