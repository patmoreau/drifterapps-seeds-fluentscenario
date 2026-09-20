using DrifterApps.Seeds.FluentScenario;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

// ReSharper disable once CheckNamespace

#pragma warning disable IDE0130
namespace FluentAssertions;
#pragma warning restore IDE0130

/// <summary>
///     Provides extension methods for asserting <see cref="Ensure{TValue}" /> instances.
/// </summary>
public static class EnsureAssertionsExtensions
{
    /// <summary>
    ///     Returns an assertion object for the specified <see cref="Ensure{TValue}" /> instance.
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
    /// <param name="instance">The result instance to assert.</param>
    /// <returns>An assertion object for the specified result instance.</returns>
    public static EnsureAssertions<TValue> Should<TValue>(this Ensure<TValue> instance) => new(instance);
}

/// <summary>
///     Provides assertion methods for <see cref="Ensure{TValue}" /> instances.
/// </summary>
public class EnsureAssertions<TValue>(Ensure<TValue> instance)
    : ReferenceTypeAssertions<Ensure<TValue>, EnsureAssertions<TValue>>(instance)
{
    /// <summary>
    ///     Gets the identifier for the assertion.
    /// </summary>
    protected override string Identifier => "ensure";

    private const string InvalidValue = "invalid value";

    /// <summary>
    ///     Gets the value to report in a failure message, without dereferencing an invalid value.
    /// </summary>
    private object? FoundValue => Subject.IsValid ? Subject.Value : InvalidValue;

    /// <summary>
    ///     Asserts that the value is valid.
    /// </summary>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="EnsureAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public AndConstraint<EnsureAssertions<TValue>> BeValid(string because = "", params object[] becauseArgs)
    {
        _ = Execute.Assertion
            .ForCondition(Subject.IsValid)
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(Identifier)
            .FailWith("Expected {context} to be valid{reason}, but it was not.");

        return new AndConstraint<EnsureAssertions<TValue>>(this);
    }

    /// <summary>
    ///     Asserts that the value is invalid.
    /// </summary>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="EnsureAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public AndConstraint<EnsureAssertions<TValue>> BeInvalid(string because = "", params object[] becauseArgs)
    {
        _ = Execute.Assertion
            .ForCondition(!Subject.IsValid)
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(Identifier)
            .FailWith("Expected {context} to be invalid{reason}, but it was.");

        return new AndConstraint<EnsureAssertions<TValue>>(this);
    }

    /// <summary>
    ///     Asserts that the value is null.
    /// </summary>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="EnsureAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public new AndConstraint<EnsureAssertions<TValue>> BeNull(string because = "", params object[] becauseArgs)
    {
        _ = Execute.Assertion
            .ForCondition(Subject is { IsNullable: true, IsValid: true, Value: null })
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(Identifier)
            .FailWith("Expected {context} to be <null>{reason}, but found {0}.", FoundValue);

        return new AndConstraint<EnsureAssertions<TValue>>(this);
    }

    /// <summary>
    ///     Asserts that the value is not null.
    /// </summary>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="EnsureAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public new AndConstraint<EnsureAssertions<TValue>> NotBeNull(string because = "", params object[] becauseArgs)
    {
        _ = Execute.Assertion
            .ForCondition(Subject is { IsValid: true, Value: not null })
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(Identifier)
            .FailWith("Expected {context} not to be <null>{reason}.");

        return new AndConstraint<EnsureAssertions<TValue>>(this);
    }

    /// <summary>
    ///     Asserts that the value has the specified value.
    /// </summary>
    /// <param name="expectedValue">The expected value.</param>
    /// <param name="because">A formatted phrase explaining why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>
    ///     <see cref="EnsureAssertions{TValue}" />
    /// </returns>
    [CustomAssertion]
    public AndConstraint<EnsureAssertions<TValue>> HaveValue(TValue expectedValue,
        string because = "", params object[] becauseArgs)
    {
        _ = Execute.Assertion
            .ForCondition(Subject.IsValid && Equals(Subject.Value, expectedValue))
            .BecauseOf(because, becauseArgs)
            .WithDefaultIdentifier(Identifier)
            .FailWith("Expected {context} to have value {0}{reason}, but found {1}.", expectedValue, FoundValue);

        return new AndConstraint<EnsureAssertions<TValue>>(this);
    }
}
