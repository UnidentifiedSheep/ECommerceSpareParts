using System.Linq.Expressions;

namespace Core.Extensions;

public static class LambdaExtensions
{
    public static string GetFieldName(this LambdaExpression keySelector)
    {
        return keySelector.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression unaryMember } => unaryMember.Member.Name,
            _ => throw new ArgumentException("Unsupported expression type for key selector")
        };
    }
}