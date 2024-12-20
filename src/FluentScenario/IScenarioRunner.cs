using System.Runtime.CompilerServices;

namespace DrifterApps.Seeds.FluentScenario;

#pragma warning disable CA1716
/// <summary>
///     Interface for running scenarios with various steps.
/// </summary>
public interface IScenarioRunner : IRunnerContext
{
    /// <summary>
    ///     Defines a given step with an action.
    /// </summary>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given(Action<IStepRunner> step);

    /// <summary>
    ///     Defines a given step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given(string description, Action step);

    /// <summary>
    ///     Defines a given step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given(string description, Func<Task> step);

    /// <summary>
    ///     Defines a given step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(string description, Action<Ensure<T>> step);

    /// <summary>
    ///     Defines a given step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(string description, Func<T> step);

    /// <summary>
    ///     Defines a given step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(string description, Func<Task<T>> step);

    /// <summary>
    ///     Defines a given step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(string description, Func<Ensure<T>, Task> step);

    /// <summary>
    ///     Defines a given step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T, T2>(string description, Func<Ensure<T>, T2> step);

    /// <summary>
    ///     Defines a given step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T, T2>(string description, Func<Ensure<T>, Task<T2>> step);

    /// <summary>
    ///     Defines a given step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given(Action step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a given step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given(Func<Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a given step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(Action<Ensure<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a given step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(Func<T> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a given step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(Func<Task<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a given step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T>(Func<Ensure<T>, Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a given step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T, T2>(Func<Ensure<T>, T2> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a given step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Given<T, T2>(Func<Ensure<T>, Task<T2>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with an action.
    /// </summary>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When(Action<IStepRunner> step);

    /// <summary>
    ///     Defines a when step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When(string description, Action step);

    /// <summary>
    ///     Defines a when step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When(string description, Func<Task> step);

    /// <summary>
    ///     Defines a when step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(string description, Action<Ensure<T>> step);

    /// <summary>
    ///     Defines a when step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(string description, Func<T> step);

    /// <summary>
    ///     Defines a when step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(string description, Func<Task<T>> step);

    /// <summary>
    ///     Defines a when step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(string description, Func<Ensure<T>, Task> step);

    /// <summary>
    ///     Defines a when step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T, T2>(string description, Func<Ensure<T>, T2> step);

    /// <summary>
    ///     Defines a when step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T, T2>(string description, Func<Ensure<T>, Task<T2>> step);

    /// <summary>
    ///     Defines a when step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When(Action step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When(Func<Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(Action<Ensure<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(Func<T> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(Func<Task<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T>(Func<Ensure<T>, Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T, T2>(Func<Ensure<T>, T2> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a when step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner When<T, T2>(Func<Ensure<T>, Task<T2>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with an action.
    /// </summary>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then(Action<IStepRunner> step);

    /// <summary>
    ///     Defines a then step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then(string description, Action step);

    /// <summary>
    ///     Defines a then step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then(string description, Func<Task> step);

    /// <summary>
    ///     Defines a then step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(string description, Action<Ensure<T>> step);

    /// <summary>
    ///     Defines a then step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(string description, Func<T> step);

    /// <summary>
    ///     Defines a then step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(string description, Func<Task<T>> step);

    /// <summary>
    ///     Defines a then step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(string description, Func<Ensure<T>, Task> step);

    /// <summary>
    ///     Defines a then step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T, T2>(string description, Func<Ensure<T>, T2> step);

    /// <summary>
    ///     Defines a then step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T, T2>(string description, Func<Ensure<T>, Task<T2>> step);

    /// <summary>
    ///     Defines a then step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then(Action step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then(Func<Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(Action<Ensure<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(Func<T> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(Func<Task<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T>(Func<Ensure<T>, Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T, T2>(Func<Ensure<T>, T2> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines a then step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner Then<T, T2>(Func<Ensure<T>, Task<T2>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with an action.
    /// </summary>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And(Action<IStepRunner> step);

    /// <summary>
    ///     Defines an and step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And(string description, Action step);

    /// <summary>
    ///     Defines an and step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And(string description, Func<Task> step);

    /// <summary>
    ///     Defines an and step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(string description, Action<Ensure<T>> step);

    /// <summary>
    ///     Defines an and step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(string description, Func<T> step);

    /// <summary>
    ///     Defines an and step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(string description, Func<Task<T>> step);

    /// <summary>
    ///     Defines an and step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(string description, Func<Ensure<T>, Task> step);

    /// <summary>
    ///     Defines an and step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T, T2>(string description, Func<Ensure<T>, T2> step);

    /// <summary>
    ///     Defines an and step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T, T2>(string description, Func<Ensure<T>, Task<T2>> step);

    /// <summary>
    ///     Defines an and step with a description and an action.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And(Action step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with a description and an asynchronous function.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And(Func<Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with a description and a generic action.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the action.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(Action<Ensure<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with a description and a generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(Func<T> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with a description and an asynchronous generic function.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(Func<Task<T>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with a description and a generic function that returns a task.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T>(Func<Ensure<T>, Task> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with a description and a function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T, T2>(Func<Ensure<T>, T2> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Defines an and step with a description and an asynchronous function with two generic parameters.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter for the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter for the function.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to execute.</param>
    /// <returns>The scenario runner instance.</returns>
    IScenarioRunner And<T, T2>(Func<Ensure<T>, Task<T2>> step, [CallerMemberName] string description = "");

    /// <summary>
    ///     Plays the scenario asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task PlayAsync();

    /// <summary>
    ///     Plays the scenario asynchronously and returns a result of type T.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>A task representing the asynchronous operation with a result of type T.</returns>
    public Task<Ensure<T>> PlayAsync<T>();
}
