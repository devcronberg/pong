using Game1;
using Xunit;

namespace Pong.Tests;

public class ScoreBoardTests
{
    [Fact]
    public void NewScoreBoard_StartsWithZeroScores()
    {
        var scoreBoard = new ScoreBoard();

        Assert.Equal(0, scoreBoard.Score1);
        Assert.Equal(0, scoreBoard.Score2);
    }

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(2, 0, 1)]
    public void AddPoint_IncrementsOnlySelectedPlayer(
        int player,
        int expectedScore1,
        int expectedScore2)
    {
        var scoreBoard = new ScoreBoard();

        scoreBoard.AddPoint(player);

        Assert.Equal(expectedScore1, scoreBoard.Score1);
        Assert.Equal(expectedScore2, scoreBoard.Score2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void AddPoint_RaisesScoreChangedWithUpdatedScore(int player)
    {
        var scoreBoard = new ScoreBoard();
        object? eventSender = null;
        ScoreChangedEventArgs? eventArgs = null;
        var eventCount = 0;
        scoreBoard.ScoreChanged += (sender, args) =>
        {
            eventSender = sender;
            eventArgs = args;
            eventCount++;
        };

        scoreBoard.AddPoint(player);
        scoreBoard.AddPoint(player);

        Assert.Equal(2, eventCount);
        Assert.Same(scoreBoard, eventSender);
        Assert.NotNull(eventArgs);
        Assert.Equal(player, eventArgs.Player);
        Assert.Equal(2, eventArgs.NewScore);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(3)]
    public void AddPoint_WithInvalidPlayer_ThrowsArgumentOutOfRangeException(int player)
    {
        var scoreBoard = new ScoreBoard();
        var eventRaised = false;
        scoreBoard.ScoreChanged += (_, _) => eventRaised = true;

        Assert.Throws<ArgumentOutOfRangeException>(() => scoreBoard.AddPoint(player));
        Assert.Equal(0, scoreBoard.Score1);
        Assert.Equal(0, scoreBoard.Score2);
        Assert.False(eventRaised);
    }
}