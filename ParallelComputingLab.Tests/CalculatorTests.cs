using ParallelComputingLab;

namespace ParallelComputingLab.Tests;

public class CalculatorTests
{
    // Контрольные сценарии из П1 (числа, кратные 13).
    [Theory]
    [InlineData(new[] { 13, 26, 40, 39 }, 78, 3)]
    [InlineData(new[] { 1, 2, 3, 4 }, 0, 0)]
    [InlineData(new[] { 13, 13, 13 }, 39, 3)]
    [InlineData(new int[] { }, 0, 0)]
    public void SequentialAndParallel_ReturnSameResult(
        int[] numbers,
        long expectedSum,
        int expectedCount)
    {
        var sequential =
            Calculator.CalculateSequential(numbers);

        var parallel =
            Calculator.CalculateParallel(numbers);

        Assert.Equal(expectedSum, sequential.Sum);
        Assert.Equal(expectedCount, sequential.Count);

        // Сравнивает значения record, а не ссылки на объекты.
        Assert.Equal(sequential, parallel);
    }

    // П1. TODO 5: объясните, какое исключение должно возникнуть,
    // и почему проверка стоит в начале метода.
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
