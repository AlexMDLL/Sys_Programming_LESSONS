using ParallelComputingLab;

namespace ParallelComputingLab.Tests;

public class CalculatorTests
{
    // Контрольные сценарии из П1 (числа, кратные 13).
    [Theory]
    [InlineData(new[] { 13, 26, 40, 39 }, 78, 3, 39)]
    [InlineData(new[] { 1, 2, 3, 4 }, 0, 0, null)]
    [InlineData(new[] { 13, 13, 13 }, 39, 3, 13)]
    [InlineData(new int[] { }, 0, 0, null)]
    [InlineData(new[] { 26 }, 26, 1, 26)]
    public void SequentialAndParallel_ReturnSameResult(
        int[] numbers,
        long expectedSum,
        int expectedCount,
        int? expectedMax)
    {
        var sequential =
            Calculator.CalculateSequential(numbers);

        var parallel =
            Calculator.CalculateParallel(numbers);

        Assert.Equal(expectedSum, sequential.Sum);
        Assert.Equal(expectedCount, sequential.Count);
        Assert.Equal(expectedMax, sequential.Max);

        // Сравнивает значения record, а не ссылки на объекты.
        Assert.Equal(sequential, parallel);
    }

    // null отклоняется сразу в начале метода, поэтому возникает
    // ArgumentNullException, а не NullReferenceException внутри Parallel.For.
    [Fact]
    public void Parallel_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(
            () => Calculator.CalculateParallel(null!));
    }

    [Fact]
    public void Parallel_WithInvalidDegree_ThrowsException()
    {
        int[] numbers = [13, 26, 39];

        Assert.Throws<ArgumentOutOfRangeException>(
            () => Calculator.CalculateParallel(numbers, 0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(-1)]
    public void Parallel_WithDifferentDegrees_ReturnsSameResult(int degree)
    {
        int[] numbers = DataGenerator.Generate(
            100_000,
            seed: 12345);

        var expected =
            Calculator.CalculateSequential(numbers);

        var actual =
            Calculator.CalculateParallel(numbers, degree);

        Assert.Equal(expected, actual);
    }
}
