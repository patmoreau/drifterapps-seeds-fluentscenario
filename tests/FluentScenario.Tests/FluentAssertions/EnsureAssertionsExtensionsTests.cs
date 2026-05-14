namespace FluentScenario.Tests.FluentAssertions;

[UnitTest]
public class EnsureAssertionsExtensionsTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void GivenBeValid_WhenIsValid_ThenShouldNotThrowException()
    {
        // Arrange
        var ensure = Ensure<int>.From(_faker.Random.Int());

        // Act
        Action act = () => ensure.Should().BeValid();

        // Assert
        act.Should().NotThrow<XunitException>();
    }

    [Theory]
    [ClassData(typeof(BecauseData))]
    public void GivenBeValid_WhenIsNotValid_ShouldThrow(string because, string[] becauseArgs)
    {
        // Arrange
        var ensure = Ensure<int>.From(null);
        var expectedMessage = GetExpectedMessage("Expected ensure to be valid{0}, but it was not.*",
            because, becauseArgs);

        // Act
        Action act = () => ensure.Should().BeValid(because, becauseArgs);

        // Assert
        act.Should().Throw<XunitException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void GivenBeInvalid_WhenNotValid_ThenShouldNotThrowException()
    {
        // Arrange
        var ensure = Ensure<int>.From(null);

        // Act
        Action act = () => ensure.Should().BeInvalid();

        // Assert
        act.Should().NotThrow<XunitException>();
    }

    [Theory]
    [ClassData(typeof(BecauseData))]
    public void GivenBeInvalid_WhenIsValid_ShouldThrow(string because, string[] becauseArgs)
    {
        // Arrange
        var ensure = Ensure<int>.From(_faker.Random.Int());
        var expectedMessage = GetExpectedMessage("Expected ensure to be invalid{0}, but it was.*",
            because, becauseArgs);

        // Act
        Action act = () => ensure.Should().BeInvalid(because, becauseArgs);

        // Assert
        act.Should().Throw<XunitException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void GivenBeNull_WhenIsNull_ThenShouldNotThrowException()
    {
        // Arrange
        var ensure = Ensure<int?>.From(null);

        // Act
        Action act = () => ensure.Should().BeNull();

        // Assert
        act.Should().NotThrow<XunitException>();
    }

    [Theory]
    [ClassData(typeof(BecauseData))]
    public void GivenBeNull_WhenIsNotNull_ShouldThrow(string because, string[] becauseArgs)
    {
        // Arrange
        var value = _faker.Random.Int();
        var ensure = Ensure<int>.From(value);
        var expectedMessage = GetExpectedMessage($"Expected ensure to be <null>{{0}}, but found {value}.*",
            because, becauseArgs);

        // Act
        Action act = () => ensure.Should().BeNull(because, becauseArgs);

        // Assert
        act.Should().Throw<XunitException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void GivenNotBeNull_WhenIsNotNull_ThenShouldNotThrowException()
    {
        // Arrange
        var ensure = Ensure<int>.From(_faker.Random.Int());

        // Act
        Action act = () => ensure.Should().NotBeNull();

        // Assert
        act.Should().NotThrow<XunitException>();
    }

    [Theory]
    [ClassData(typeof(BecauseData))]
    public void GivenNotBeNull_WhenIsNull_ShouldThrow(string because, string[] becauseArgs)
    {
        // Arrange
        var ensure = Ensure<int?>.From(null);
        var expectedMessage = GetExpectedMessage("Expected ensure not to be <null>{0}.*",
            because, becauseArgs);

        // Act
        Action act = () => ensure.Should().NotBeNull(because, becauseArgs);

        // Assert
        act.Should().Throw<XunitException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void GivenHaveValue_WhenIsSame_ThenShouldNotThrowException()
    {
        // Arrange
        var value = _faker.Random.Int();
        var ensure = Ensure<int>.From(value);

        // Act
        Action act = () => ensure.Should().HaveValue(value);

        // Assert
        act.Should().NotThrow<XunitException>();
    }

    [Theory]
    [ClassData(typeof(BecauseData))]
    public void GivenHaveValue_WhenNotSame_ShouldThrow(string because, string[] becauseArgs)
    {
        // Arrange
        var value = _faker.Random.Int();
        var expectedValue = _faker.Random.Int();
        var ensure = Ensure<int>.From(value);
        var expectedMessage = GetExpectedMessage($"Expected ensure to have value {expectedValue}{{0}}, but found {value}.*",
            because, becauseArgs);

        // Act
        Action act = () => ensure.Should().HaveValue(expectedValue, because, becauseArgs);

        // Assert
        act.Should().Throw<XunitException>().WithMessage(expectedMessage);
    }

    private static string GetExpectedMessage(string messageFormat, string because, params string[] becauseArgs)
    {
        var expectedBecause = string.Format(CultureInfo.InvariantCulture, because, becauseArgs);
        return string.Format(CultureInfo.InvariantCulture, messageFormat,
            string.IsNullOrWhiteSpace(expectedBecause) ? "" : $" because {expectedBecause}");
    }

    internal class BecauseData : TheoryData<string, string[]>
    {
        public BecauseData()
        {
            Add("", []);
            var faker = new Faker();
            var because = faker.Lorem.Sentence();
            Add(because, faker.Lorem.Words(2));
            Add(because + "'{0};{1}'", faker.Lorem.Words(2));
        }
    }
}
