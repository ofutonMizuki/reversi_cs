using System;
using System.Linq;
using Xunit;

namespace reversi_cs.Tests
{
    public class GameLogicMoveTests
    {
        [Fact]
        public void IsValidMove_OutOfBounds_ShouldBeFalse()
        {
            var game = TestHelpers.NewGame();

            Assert.False(game.IsValidMove(-1, 0, Stone.BLACK));
            Assert.False(game.IsValidMove(0, -1, Stone.BLACK));
            Assert.False(game.IsValidMove(8, 0, Stone.BLACK));
            Assert.False(game.IsValidMove(0, 8, Stone.BLACK));
        }

        [Fact]
        public void IsValidMove_OccupiedSquare_ShouldBeFalse()
        {
            var game = TestHelpers.NewGame();

            Assert.False(game.IsValidMove(3, 3, Stone.BLACK));
            Assert.False(game.IsValidMove(3, 4, Stone.WHITE));
        }

        [Fact]
        public void GetValidMoves_ShouldMatchIsValidMoveGridScan()
        {
            var game = TestHelpers.NewGame();

            var moves = game.GetValidMoves(Stone.BLACK).ToHashSet();

            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    bool valid = game.IsValidMove(x, y, Stone.BLACK);
                    Assert.Equal(valid, moves.Contains((x, y)));
                }
        }

        [Fact]
        public void GetValidMoves_WhenNoMoves_ShouldBeEmpty()
        {
            var game = TestHelpers.NewGame();

            // All black board -> white has no moves.
            game.SetPositionForTest(new BitBoard(~0UL, 0UL), Stone.WHITE);

            var moves = game.GetValidMoves(Stone.WHITE);
            Assert.Empty(moves);
            Assert.False(game.HasAnyValidMove(Stone.WHITE));
        }

        [Fact]
        public void TryPlaceAt_OnOccupiedSquare_ShouldBeFalseAndNotChangeTurn()
        {
            var game = TestHelpers.NewGame();
            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());

            bool placed = game.TryPlaceAt(3, 3);
            Assert.False(placed);
            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());
        }

        [Fact]
        public void TryPlaceAt_OutOfBounds_ShouldBeFalseAndNotChangeTurn()
        {
            var game = TestHelpers.NewGame();
            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());

            bool placed = game.TryPlaceAt(-1, -1);
            Assert.False(placed);
            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());
        }
    }
}
