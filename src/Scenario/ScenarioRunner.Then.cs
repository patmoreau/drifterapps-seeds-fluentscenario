namespace DrifterApps.Seeds.Scenario;

internal sealed partial class ScenarioRunner
{
    /// <summary>
    ///     Adds a step to be executed in the scenario.
    /// </summary>
    /// <param name="step">The action to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then(Action<IStepRunner> step)
    {
        ArgumentNullException.ThrowIfNull(step);

        _stepCommand = nameof(Then);

        step.Invoke(this);

        return this;
    }

    /// <summary>
    ///     Adds a step with a description to be executed in the scenario.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then(string description, Action step)
    {
        AddStep(nameof(Then), description, async _ =>
        {
            await Task.Run(step).ConfigureAwait(false);
            return null;
        });

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous step with a description to be executed in the scenario.
    /// </summary>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then(string description, Func<Task> step)
    {
        AddStep(nameof(Then), description, async _ =>
        {
            await step().ConfigureAwait(false);
            return null;
        });

        return this;
    }

    /// <summary>
    ///     Adds a step with a description and a parameter to be executed in the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The action to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then<T>(string description, Action<Ensure<T>> step)
    {
        AddStep(nameof(Then), description, async input =>
        {
            await Task.Run(() => step(Ensure<T>.From(input))).ConfigureAwait(false);
            return null;
        });

        return this;
    }

    /// <summary>
    ///     Adds a step with a description and a return value to be executed in the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then<T>(string description, Func<T> step)
    {
        AddStep(nameof(Then), description, async _ => await Task.Run(step).ConfigureAwait(false));

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous step with a description and a return value to be executed in the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then<T>(string description, Func<Task<T>> step)
    {
        AddStep(nameof(Then), description, async _ => await step().ConfigureAwait(false));

        return this;
    }

    /// <summary>
    ///     Adds a step with a description, a parameter, and a return value to be executed in the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <typeparam name="T2">The type of the return value.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The function to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then<T, T2>(string description, Func<Ensure<T>, T2> step)
    {
        AddStep(nameof(Then), description,
            async input => await Task.Run(() => step(Ensure<T>.From(input))).ConfigureAwait(false));

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous step with a description, a parameter, and a return value to be executed in the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <typeparam name="T2">The type of the return value.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then<T, T2>(string description, Func<Ensure<T>, Task<T2>> step)
    {
        AddStep(nameof(Then), description,
            async input => await step(Ensure<T>.From(input)).ConfigureAwait(false));

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous step with a description and a parameter to be executed in the scenario.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="description">The description of the step.</param>
    /// <param name="step">The asynchronous function to be executed as a step.</param>
    /// <returns>The current instance of <see cref="IScenarioRunner" />.</returns>
    public IScenarioRunner Then<T>(string description, Func<Ensure<T>, Task> step)
    {
        AddStep(nameof(Then), description, async input =>
        {
            await step(Ensure<T>.From(input)).ConfigureAwait(false);
            return null;
        });

        return this;
    }
}
