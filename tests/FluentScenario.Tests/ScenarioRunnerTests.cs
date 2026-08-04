using FluentScenario.Tests.Extensions;
using FluentScenario.Tests.Mocks;

namespace FluentScenario.Tests;

[UnitTest]
public class ScenarioRunnerTests
{
    private readonly IScenarioOutput _scenarioOutput = Substitute.For<IScenarioOutput>();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenDescription_WhenNull_ThenThrowArgumentNullException(string? description)
    {
        // arrange

        // act
        Action act = () => ScenarioRunner.Create(description!, _scenarioOutput);

        // assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("Please explain your intent by documenting your scenario.*");
    }

    [Fact]
    public async Task GivenScenario_WhenExceptionThrown_ThenOutputFailure()
    {
        // arrange
        var sut = ScenarioRunner.Create(_scenarioOutput)
            .Given("Test failure", () => throw new InvalidOperationException("Test exception"));

        // act
        var action = sut.PlayAsync;

        // assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
        _scenarioOutput.Received().WriteLine(
            Arg.Is<string>(x => x != null && x.Contains($"{ScenarioRunner.FailCheck} GIVEN Test failure")));
    }

    [Fact]
    public async Task GivenScenario_WhenNoDescription_ThenScenarioIsMemberName()
    {
        // arrange
        var sut = ScenarioRunner.Create(_scenarioOutput);

        // act
        await sut.PlayAsync();

        // assert
        _scenarioOutput.Received().WriteLine(
            Arg.Is<string>(x => x != null && x.Contains("given scenario when no description then scenario is member name")));
    }

    [Fact]
    public async Task GivenAction_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", _scenarioOutput)
            .Given("Task 1: Given Action", () => { ++count; })
            .And("Task 2: And Action", () => { ++count; })
            .When("Task 3: When Action", () => { ++count; })
            .Then("Task 4: Then Action", () => { ++count; });

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(4);
    }

    [Fact]
    public async Task GivenActionWithNoDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(_scenarioOutput)
            .Given(() => { ++count; })
            .And(() => { ++count; })
            .When(() => { ++count; })
            .Then(() => { ++count; });

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(4);
    }

    [Fact]
    public async Task GivenFuncOfTask_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", _scenarioOutput)
            .Given("Task 1: Given Func<Task>", () => Task.Run(() => { ++count; }))
            .And("Task 2: And Func<Task>", () => Task.Run(() => { ++count; }))
            .When("Task 3: When Func<Task>", () => Task.Run(() => { ++count; }))
            .Then("Task 5: Then Func<Task>", () => Task.Run(() => { ++count; }));

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(4);
    }

    [Fact]
    public async Task GivenFuncOfTaskWithoutDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(_scenarioOutput)
            .Given(() => Task.Run(() => { ++count; }))
            .And(() => Task.Run(() => { ++count; }))
            .When(() => Task.Run(() => { ++count; }))
            .Then(() => Task.Run(() => { ++count; }));

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(4);
    }

    [Fact]
    public async Task GivenActionOfT_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", count, _scenarioOutput)
            .Given("Task 1: Given Action<T>", (Ensure<int> number) => { count += number.Value + 10; })
            .And("Task 0: Setup Func<T>", () => count)
            .And("Task 2: And Action<T>", (Ensure<int> number) => { count += number.Value + 10; })
            .And("Task 0: Setup Func<T>", () => count)
            .When("Task 3: When Action<T>", (Ensure<int> number) => { count += number.Value + 10; })
            .And("Task 0: Setup Func<T>", () => count)
            .Then("Task 5: Then Action<T>", (Ensure<int> number) => { count += number.Value + 10; });

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(150);
    }

    [Fact]
    public async Task GivenActionOfTWithoutDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(count, _scenarioOutput)
            .Given((Ensure<int> number) => { count += number.Value + 10; })
            .And(() => count)
            .And((Ensure<int> number) => { count += number.Value + 10; })
            .And(() => count)
            .When((Ensure<int> number) => { count += number.Value + 10; })
            .And(() => count)
            .Then((Ensure<int> number) => { count += number.Value + 10; });

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(150);
    }

    [Fact]
    public async Task GivenFuncOfT_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", _scenarioOutput)
            .Given("Task 1: Given Func<T>", () => ++count)
            .And("Task 2: And Func<T>", () => ++count)
            .When("Task 3: When Func<T>", () => ++count)
            .Then("Task 5: Then Func<T>", () => ++count);

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(4);
    }

    [Fact]
    public async Task GivenFuncOfTWithoutDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(_scenarioOutput)
            .Given(() => ++count)
            .And(() => ++count)
            .When(() => ++count)
            .Then(() => ++count);

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(4);
    }

    [Fact]
    public async Task GivenFuncOfTaskT_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", _scenarioOutput)
            .Given("Task 1: Given Func<Task<T>>", () => Task.Run(() => ++count))
            .And("Task 2: And Func<Task<T>>", () => Task.Run(() => ++count))
            .When("Task 3: When Func<Task<T>>", () => Task.Run(() => ++count))
            .Then("Task 5: Then Func<Task<T>>", () => Task.Run(() => ++count));

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(4);
    }

    [Fact]
    public async Task GivenFuncOfTaskTWithoutDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(_scenarioOutput)
            .Given(() => Task.Run(() => ++count))
            .And(() => Task.Run(() => ++count))
            .When(() => Task.Run(() => ++count))
            .Then(() => Task.Run(() => ++count));

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(4);
    }

    [Fact]
    public async Task GivenFuncOfTAndTask_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", count, _scenarioOutput)
            .Given("Task 1: Given Func<Ensure<T>, Task>", (Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }))
            .And("Task Setup: Func<T>", () => count)
            .And("Task 2: And Func<Ensure<T>, Task", (Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }))
            .And("Task Setup: Func<T>", () => count)
            .When("Task 3: When Func<Ensure<T>, Task", (Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }))
            .And("Task Setup: Func<T>", () => count)
            .Then("Task 5: Then Func<Ensure<T>, Task", (Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }));

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(150);
    }

    [Fact]
    public async Task GivenFuncOfTAndTaskWithoutDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(count, _scenarioOutput)
            .Given((Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }))
            .And(() => count)
            .And((Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }))
            .And(() => count)
            .When((Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }))
            .And(() => count)
            .Then((Ensure<int> number) => Task.Run(() => { count += number.Value + 10; }));

        // act
        await sut.PlayAsync();

        // assert
        count.Should().Be(150);
    }

    [Fact]
    public async Task GivenFuncOfTAndT2_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", count, _scenarioOutput)
            .Given("Task 1: Given Func<Ensure<T>, T2>", (Ensure<int> number) => count += number.Value + 10)
            .And("Task 2: And Func<Ensure<T>, T2>", (Ensure<int> number) => count += number.Value + 10)
            .When("Task 3: When Func<Ensure<T>, T2>", (Ensure<int> number) => count += number.Value + 10)
            .Then("Task 4: Then Func<Ensure<T>, T2>", (Ensure<int> number) => count += number.Value + 10);

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(150);
    }

    [Fact]
    public async Task GivenFuncOfTAndT2WithoutDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(count, _scenarioOutput)
            .Given((Ensure<int> number) => count += number.Value + 10)
            .And((Ensure<int> number) => count += number.Value + 10)
            .When((Ensure<int> number) => count += number.Value + 10)
            .Then((Ensure<int> number) => count += number.Value + 10);

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(150);
    }

    [Fact]
    public async Task GivenFuncOfTAndTaskT2_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create("Testing Action Scenario", count, _scenarioOutput)
            .Given("Task 1: Given Func<Ensure<T>, Task<T2>>", (Ensure<int> number) => Task.Run(() => count += number.Value + 10))
            .And("Task 2: And Func<Ensure<T>, Task<T2>>", (Ensure<int> number) => Task.Run(() => count += number.Value + 10))
            .When("Task 3: When Func<Ensure<T>, Task<T2>>", (Ensure<int> number) => Task.Run(() => count += number.Value + 10))
            .Then("Task 4: Then Func<Ensure<T>, Task<T2>>", (Ensure<int> number) => Task.Run(() => count += number.Value + 10));

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(150);
    }

    [Fact]
    public async Task GivenFuncOfTAndTaskT2WithoutDescription_WhenRunAsync_ThenShouldRunSteps()
    {
        // arrange
        var count = 0;
        var sut = ScenarioRunner.Create(count, _scenarioOutput)
            .Given((Ensure<int> number) => Task.Run(() => count += number.Value + 10))
            .And((Ensure<int> number) => Task.Run(() => count += number.Value + 10))
            .When((Ensure<int> number) => Task.Run(() => count += number.Value + 10))
            .Then((Ensure<int> number) => Task.Run(() => count += number.Value + 10));

        // act
        var result = await sut.PlayAsync<int>();

        // assert
        result.Should().BeValid().And.HaveValue(150);
    }
    public const string OutputFromExecute = "Executing...";

    [Theory]
    [ClassData(typeof(AndRunnerData))]
    public async Task OutputTests(string methodName, MockScenarioOutput output,
        object step)
    {
        // arrange
        var description = "description";
        output.Reset();
        var sut = ScenarioRunner.Create(output);

        // act
        sut.InvokeStep(methodName, description, step);
        sut.InvokeStep("And", description, step);
        sut.InvokeStep(methodName, step);
        sut.InvokeStep("And", step);
        await sut.PlayAsync();

        // assert
        var upperMethodName = methodName == "And" ? "GIVEN" : methodName.ToUpper(CultureInfo.InvariantCulture);
        _ = output.Messages.Should().HaveCount(9).And.ContainInOrder(
            $"{ScenarioRunner.SuccessCheck} SCENARIO for output tests",
            OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} {upperMethodName} {description}",
            OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} and {description}",
            OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} and caller member name attribute",
            OutputFromExecute,
            $"{ScenarioRunner.SuccessCheck} and caller member name attribute"
            );
    }

    internal class AndRunnerData : TheoryData<string, MockScenarioOutput, object>
    {
        private readonly MockScenarioOutput _scenarioOutput = new();

        public AndRunnerData()
        {
            Add("Given", _scenarioOutput, TestData.ExecuteAction(_scenarioOutput));
            Add("Given", _scenarioOutput, TestData.ExecuteActionOfT(_scenarioOutput));
            Add("Given", _scenarioOutput, TestData.ExecuteFuncTask(_scenarioOutput));
            Add("Given", _scenarioOutput, TestData.ExecuteFuncOfT(_scenarioOutput));
            Add("Given", _scenarioOutput, TestData.ExecuteFuncTaskOfT(_scenarioOutput));
            Add("Given", _scenarioOutput, TestData.ExecuteFuncTAndTask(_scenarioOutput));
            Add("Given", _scenarioOutput, TestData.ExecuteFuncTAndT(_scenarioOutput));
            Add("Given", _scenarioOutput, TestData.ExecuteFuncTAndTaskOfT(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteAction(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteActionOfT(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteFuncTask(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteFuncOfT(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteFuncTaskOfT(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteFuncTAndTask(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteFuncTAndT(_scenarioOutput));
            Add("When", _scenarioOutput, TestData.ExecuteFuncTAndTaskOfT(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteAction(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteActionOfT(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteFuncTask(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteFuncOfT(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteFuncTaskOfT(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteFuncTAndTask(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteFuncTAndT(_scenarioOutput));
            Add("Then", _scenarioOutput, TestData.ExecuteFuncTAndTaskOfT(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteAction(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteActionOfT(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteFuncTask(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteFuncOfT(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteFuncTaskOfT(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteFuncTAndTask(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteFuncTAndT(_scenarioOutput));
            Add("And", _scenarioOutput, TestData.ExecuteFuncTAndTaskOfT(_scenarioOutput));
        }
    }
}
