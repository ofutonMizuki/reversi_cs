using Xunit;

namespace reversi_cs.Tests
{
    public class GameLogicGameOverTests
    {
        [Fact]
        public void GameOver_WhenBothPlayersHaveNoMoves_ShouldBeSetAfterPassAttempt()
        {
            string? lastMessage = null;
            var game = new GameLogic(_ => { }, msg => lastMessage = msg);
            game.InitializeGame();

            // Full board -> no moves for either.
            var bb = new BitBoard(~0UL, 0UL);
            game.SetPositionForTest(bb, Stone.BLACK);

            Assert.False(game.HasAnyValidMove(Stone.BLACK));
            Assert.False(game.HasAnyValidMove(Stone.WHITE));

            // Trigger the pass/game-over path.
            bool placed = game.TryPlaceAt(0, 0);
            Assert.False(placed);

            Assert.True(game.IsGameOver());
            Assert.NotNull(lastMessage);
            Assert.Contains("Game Over", lastMessage);
        }

        [Fact]
        public void GameOver_ShouldRejectFurtherMoves()
        {
            var game = new GameLogic(_ => { }, _ => { });
            game.InitializeGame();

            // Full board -> no moves for either.
            game.SetPositionForTest(new BitBoard(~0UL, 0UL), Stone.BLACK);

            _ = game.TryPlaceAt(0, 0);
            Assert.True(game.IsGameOver());

            Assert.False(game.TryPlaceAt(2, 4));
        }
    }
}
