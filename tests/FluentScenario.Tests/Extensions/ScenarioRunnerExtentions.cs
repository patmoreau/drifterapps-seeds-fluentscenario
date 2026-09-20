using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FluentScenario.Tests.Extensions;

internal static class ScenarioRunnerExtensions
{
    public static IScenarioRunner InvokeStep(this IScenarioRunner runner, string methodName, params object[] parameters)
    {
        var usingDescription = parameters[0] is string;
        var stepType = usingDescription ? parameters[1].GetType() : parameters[0].GetType();
        var method = typeof(IScenarioRunner).GetMethods()
            .Where(mi => mi.Name == methodName)
            .Where(IsNotStepRunnerSignature)
            .Where(mi => IsUsingDescription(mi, usingDescription))
            .First(mi => IsSameStepSignature(mi, stepType, usingDescription));

        var methodCall = method ?? throw new InvalidOperationException(
            $"Method {methodName} with {parameters.Length} parameters not found.");
        if (IsActionOfT(stepType) || IsFuncTaskOfT(stepType) || IsFuncOfT(stepType) || IsFuncOfTAndTask(stepType))
        {
            methodCall = MakeGenericOfT(method, stepType);
        }
        else if (IsFuncOfTAndTaskOfT(stepType) || IsFuncOfTAndT(stepType))
        {
            methodCall = MakeGenericOfTAndT(method, stepType);
        }

        if (methodCall.GetParameters()[^1].GetCustomAttributes<CallerMemberNameAttribute>().Any())
        {
            parameters = [.. parameters, nameof(CallerMemberNameAttribute)];
        }

        return (IScenarioRunner)methodCall.Invoke(runner, parameters)!;
    }

    private static bool IsNotStepRunnerSignature(MethodInfo method) => method.GetParameters().Length > 1;

    private static bool IsUsingDescription(MethodInfo method, bool usingDescription) =>
        (usingDescription && method.GetParameters()[0].ParameterType == typeof(string)) ||
        (!usingDescription && method.GetParameters()[0].ParameterType != typeof(string));

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
    private static bool IsSameStepSignature(MethodInfo method, Type stepType, bool usingDescription)
    {
        var methodType = usingDescription
            ? method.GetParameters()[1].ParameterType
            : method.GetParameters()[0].ParameterType;

        if (IsAction(methodType))
        {
            return IsAction(stepType);
        }

        if (IsActionOfT(methodType))
        {
            return IsActionOfT(stepType);
        }

        if (IsFuncTask(methodType))
        {
            return IsFuncTask(stepType);
        }

        if (IsFuncTaskOfT(methodType))
        {
            return IsFuncTaskOfT(stepType);
        }

        if (IsFuncOfT(methodType))
        {
            return IsFuncOfT(stepType);
        }

        if (IsFuncOfTAndTask(methodType))
        {
            return IsFuncOfTAndTask(stepType);
        }

        if (IsFuncOfTAndTaskOfT(methodType))
        {
            return IsFuncOfTAndTaskOfT(stepType);
        }

        if (IsFuncOfTAndT(methodType))
        {
            return IsFuncOfTAndT(stepType);
        }

        return false;
    }

    private static bool IsAction(Type type) => type == typeof(Action);

    private static bool IsActionOfT(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Action<>);

    private static bool IsFuncTask(Type type) => type == typeof(Func<Task>);

    private static bool IsFuncTaskOfT(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>) &&
        IsParamGenericTask(0, type.GetGenericArguments());

    private static bool IsFuncOfT(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>) &&
        type != typeof(Func<Task>) &&
        !IsParamGenericTask(0, type.GetGenericArguments());

    private static bool IsFuncOfTAndTask(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,>) &&
        type.GetGenericArguments()[1] == typeof(Task);

    private static bool IsFuncOfTAndT(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,>) &&
        type.GetGenericArguments()[1] != typeof(Task) &&
        !IsParamGenericTask(1, type.GetGenericArguments());

    private static bool IsFuncOfTAndTaskOfT(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,>) &&
        IsParamGenericTask(1, type.GetGenericArguments());

    private static bool IsParamGenericTask(int index, params Type[] args) =>
        index < args.Length && args[index].IsGenericType && args[index].GetGenericTypeDefinition() == typeof(Task<>);

    private static MethodInfo MakeGenericOfT(MethodInfo method, Type objType)
    {
        var genType = GetGenericType(objType.GetGenericArguments()[0]);
        return method.MakeGenericMethod(genType);
    }

    private static MethodInfo MakeGenericOfTAndT(MethodInfo method, Type objType)
    {
        var genType1 = GetGenericType(objType.GetGenericArguments()[0]);
        var genType2 = GetGenericType(objType.GetGenericArguments()[1]);
        return method.MakeGenericMethod(genType1, genType2);
    }

    private static Type GetGenericType(Type objType) =>
        objType.IsGenericType ? GetGenericType(objType.GetGenericArguments()[0]) : objType;
}
