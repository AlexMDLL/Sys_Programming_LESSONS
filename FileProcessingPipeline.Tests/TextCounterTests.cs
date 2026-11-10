using FileProcessingPipeline.Services;
namespace FileProcessingPipeline.Tests;

public class TextCounterTests
{
    [Fact]
    public void EmptyText_HasNoLinesWordsOrCharacters()
    {
        var statistics = TextCounter.Count("");
        Assert.Equal(0, statistics.Lines);
        Assert.Equal(0, statistics.Words);
        Assert.Equal(0, statistics.Characters);
    }

    [Fact]
    public void TextWithTrailingNewline_CountsOneLine()
    {
        var statistics = TextCounter.Count("Hello\n");
        Assert.Equal(1, statistics.Lines);
        Assert.Equal(1, statistics.Words);
        Assert.Equal(6, statistics.Characters);
    }

    [Fact]
    public void MultiLineText_CountsLinesAndWords()
    {
        var statistics = TextCounter.Count("one two\nthree four\n");
        Assert.Equal(2, statistics.Lines);
        Assert.Equal(4, statistics.Words);
    }
}
