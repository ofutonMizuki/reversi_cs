using Xunit;

namespace reversi_cs.Tests
{
    public class GameLogicPassTests
    {
        [Fact]
        public void TryPlaceAt_WhenCurrentPlayerHasNoMoves_ShouldPassToOpponent()
        {
            var game = new GameLogic(_ => { }, _ => { });
            game.InitializeGame();

            // Construct a position where White has no moves, Black has moves.
            // Row y=0: "BW......" -> Black can play at (2,0) to flip (1,0).
            // White has only one stone and cannot flip anything.
            var bb = TestHelpers.BoardFromStrings(
                "BW......",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........");

            game.SetPositionForTest(bb, Stone.WHITE);

            Assert.False(game.HasAnyValidMove(Stone.WHITE));
            Assert.True(game.HasAnyValidMove(Stone.BLACK));

            _ = game.TryPlaceAt(-1, -1);

            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());
        }

        [Fact]
        public void TryPlaceAt_WhenPlayerHasMoves_InvalidMoveShouldNotPass()
        {
            var game = TestHelpers.NewGame();
            Assert.True(game.HasAnyValidMove(Stone.BLACK));

            // Black has legal moves in initial position; invalid move must NOT pass.
            bool placed = game.TryPlaceAt(0, 0);
            Assert.False(placed);
            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());
        }
    }
}
