using System.Linq;
using Xunit;

namespace reversi_cs.Tests;

public class GameLogicBasicTests
{
    [Fact]
    public void InitialPosition_ShouldHaveTwoStonesEach()
    {
        var game = TestHelpers.NewGame();

        var (b, w) = game.CountStones();
        Assert.Equal(2, b);
        Assert.Equal(2, w);
    }

    [Fact]
    public void InitialPosition_BlackShouldHaveFourLegalMoves()
    {
        var game = TestHelpers.NewGame();

        var moves = game.GetValidMoves(reversi_cs.Stone.BLACK);
        TestHelpers.AssertMovesEqual(moves, (2, 4), (3, 5), (4, 2), (5, 3));
    }

    [Fact]
    public void InitialPosition_WhiteShouldHaveFourLegalMoves()
    {
        var game = TestHelpers.NewGame();

        var moves = game.GetValidMoves(reversi_cs.Stone.WHITE);
        TestHelpers.AssertMovesEqual(moves, (2, 3), (3, 2), (4, 5), (5, 4));
    }

    [Fact]
    public void ApplyMove_BlackAt_2_4_ShouldFlipOneStoneAndSwitchTurn()
    {
        var game = TestHelpers.NewGame();

        Assert.Equal(reversi_cs.Stone.BLACK, game.GetCurrentPlayer());

        bool placed = game.TryPlaceAt(2, 4);
        Assert.True(placed);

        var (b, w) = game.CountStones();
        Assert.Equal(4, b);
        Assert.Equal(1, w);

        Assert.Equal(reversi_cs.Stone.WHITE, game.GetCurrentPlayer());
    }
}
