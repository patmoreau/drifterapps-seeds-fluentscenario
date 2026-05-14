namespace FluentScenario.Tests.Samples;

/// <summary>
/// Depending of the test framework you use, you can create a custom implementation of IScenarioOutput to write the
/// output to the test output.
/// </summary>
/// <remarks>This is for tests running under xUnit.</remarks>
/// <param name="testOutputHelper"><see cref="ITestOutputHelper"/></param>
public class SampleScenarioOutput(ITestOutputHelper testOutputHelper) : IScenarioOutput
{
    /// <inheritdoc />
    public void WriteLine(string message) => testOutputHelper.WriteLine(message);

    /// <inheritdoc />
    public void WriteLine(string format, params object[] args) => testOutputHelper.WriteLine(format, args);
}
