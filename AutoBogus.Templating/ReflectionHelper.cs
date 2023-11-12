using System.Linq.Expressions;
using System.Reflection;

namespace AutoBogus.Templating;

internal static class ReflectionHelper
{
    public static Type GetPropType(object src, string propName)
    {
        return src.GetType().GetProperty(propName)!.PropertyType;
    }

    public static MethodInfo GetMethod(Type t, string n, Type[] genargs, Type[] args)
    {
        var methods = t.GetMethods()
            .Where(it => it.Name == n && it.GetGenericArguments().Length == genargs.Length)
            .Select(it => it.IsGenericMethodDefinition ? it.MakeGenericMethod(genargs) : it)
            .Where(it => it.GetParameters().Select(p => p.ParameterType).SequenceEqual(args))
            .Select(it => it)
            .ToList();

        return methods.Single();
    }

    public static string GetMemberName(Expression expression)
    {
        var memberExpression = expression is UnaryExpression unaryExpression
            ? unaryExpression.Operand as MemberExpression
            : expression as MemberExpression;

        if (memberExpression == null)
        {
            throw new ArgumentException("Expression was not of the form 'x => x.Property or x => x.Field'.");
        }

        return memberExpression.Member.Name;
    }

    public static object? GetPropValue(object src, string propName)
    {
        return src.GetType().GetProperty(propName)?.GetValue(src, null);
    }
}
