using System.Diagnostics.CodeAnalysis;
using FluentScenario.Tests.Mocks;

namespace FluentScenario.Tests;

[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
[UnitTest]
public class StepRunnerTests
{
    [Theory]
    [ClassData(typeof(StepRunnerData))]
    public async Task GivenExecute_WhenPlayAsync_ThenShouldRunSteps(string testName, MockScenarioOutput output,
        Action<IStepRunner> step)
    {
        // arrange
        output.Reset();
        var sut = ScenarioRunner.Create("Testing StepRunner Execute", output)
            .Given(step)
            .And(step)
            .When(step)
            .And(step)
            .Then(step)
            .And(step);

        // act
        await sut.PlayAsync();

        // assert
        _ = output.Messages.Should().HaveCount(13).And.ContainInOrder(
            $"{ScenarioRunner.SuccessCheck} SCENARIO for Testing StepRunner Execute",
            TestData.OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} GIVEN {testName}",
            TestData.OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} and {testName}",
            TestData.OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} WHEN {testName}",
            TestData.OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} and {testName}",
            TestData.OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} THEN {testName}",
            TestData.OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} and {testName}"
            );
    }

    [Theory]
    [ClassData(typeof(StepRunnerData))]
    public async Task GivenExecute_WhenAndIsFirst_ThenShouldRunAsGivenSteps(string testName, MockScenarioOutput output,
        Action<IStepRunner> step)
    {
        // arrange
        output.Reset();
        var sut = ScenarioRunner.Create("Testing StepRunner Execute", output)
            .And(step);

        // act
        await sut.PlayAsync();

        // assert
        _ = output.Messages.Should().HaveCount(3).And.ContainInOrder(
            $"{ScenarioRunner.SuccessCheck} SCENARIO for Testing StepRunner Execute",
            TestData.OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} GIVEN {testName}"
        );
    }

    [Fact]
    public async Task GivenExecute_WhenAssertionThrown_ThenXUnitCatchesFailure()
    {
        // arrange
        var output = new MockScenarioOutput();
        var expectedValue = 42;
        var actualValue = 99;
        var sut = ScenarioRunner.Create("Testing assertion propagation", output)
            .Given(runner => runner.Execute("value should be 42", () => actualValue.Should().Be(expectedValue)));

        // act
        var action = () => sut.PlayAsync();

        // assert
        await action.Should().ThrowAsync<XunitException>()
            .WithMessage($"Expected actualValue to be {expectedValue}, but found {actualValue}*");

        output.Messages.Should().Contain($"{ScenarioRunner.FailCheck} GIVEN value should be 42");
    }

    [Fact]
    public async Task GivenExecute_WhenAssertionThrownInAction_ThenXUnitCatchesFailure()
    {
        // arrange
        var output = new MockScenarioOutput();
        var sut = ScenarioRunner.Create("Testing assertion in Action", output)
            .Given(runner => runner.Execute("should fail", () =>
            {
                1.Should().Be(2, "this assertion should be caught by xUnit");
            }));

        // act
        var action = () => sut.PlayAsync();

        // assert
        await action.Should().ThrowAsync<XunitException>()
            .WithMessage("*this assertion should be caught by xUnit*");
    }

    [Fact]
    public async Task GivenExecute_WhenAssertionThrownInActionWithEnsure_ThenXUnitCatchesFailure()
    {
        // arrange
        var output = new MockScenarioOutput();
        var sut = ScenarioRunner.Create("Testing assertion in Action<Ensure<T>>", output)
            .Given("initial value", () => 100)
            .When(runner => runner.Execute<int>("value should be less than 50", ensure =>
            {
                ensure.Value.Should().BeLessThan(50, "value must be under threshold");
            }));

        // act
        var action = () => sut.PlayAsync();

        // assert
        await action.Should().ThrowAsync<XunitException>()
            .WithMessage("*value must be under threshold*");
    }

    internal class StepRunnerData : TheoryData<string, MockScenarioOutput, Action<IStepRunner>>
    {
        private readonly MockScenarioOutput _scenarioOutput = new();

        public StepRunnerData()
        {
            AddStep("Execute Action", runner => runner.Execute("Execute Action", TestData.ExecuteAction(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteAction(_scenarioOutput)));
            AddStep("Execute Func<Task>", runner => runner.Execute("Execute Func<Task>", TestData.ExecuteFuncTask(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteFuncTask(_scenarioOutput)));
            AddStep("Execute Action<Ensure<T>>", runner => runner.Execute("Execute Action<Ensure<T>>", TestData.ExecuteActionOfT(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteActionOfT(_scenarioOutput)));
            AddStep("Execute Func<T>", runner => runner.Execute("Execute Func<T>", TestData.ExecuteFuncOfT(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteFuncOfT(_scenarioOutput)));
            AddStep("Execute Func<Task<T>>", runner => runner.Execute("Execute Func<Task<T>>", TestData.ExecuteFuncTaskOfT(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteFuncTaskOfT(_scenarioOutput)));
            AddStep("Execute Func<Ensure<T>, Task>", runner => runner.Execute("Execute Func<Ensure<T>, Task>", TestData.ExecuteFuncTAndTask(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteFuncTAndTask(_scenarioOutput)));
            AddStep("Execute Func<Ensure<T>, T>", runner => runner.Execute("Execute Func<Ensure<T>, T>", TestData.ExecuteFuncTAndT(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteFuncTAndT(_scenarioOutput)));
            AddStep("Execute Func<Ensure<T>, Task<T>>", runner => runner.Execute("Execute Func<Ensure<T>, Task<T>>", TestData.ExecuteFuncTAndTaskOfT(_scenarioOutput)));
            AddStep(".ctor", runner => runner.Execute(TestData.ExecuteFuncTAndTaskOfT(_scenarioOutput)));
        }

        private void AddStep(string name, Action<IStepRunner> step) => Add(name, _scenarioOutput, step);
    }
}
