using Xunit;

namespace reversi_cs.Tests
{
    public class GameLogicPlaySequenceTests
    {
        [Fact]
        public void AfterTwoPlies_CountsShouldBeConsistent()
        {
            var game = TestHelpers.NewGame();

            // Black
            Assert.True(game.TryPlaceAt(2, 4));
            Assert.Equal(Stone.WHITE, game.GetCurrentPlayer());
            Assert.Equal((4, 1), game.CountStones());

            // White responds (a legal reply)
            Assert.True(game.TryPlaceAt(2, 3));
            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());

            var (b, w) = game.CountStones();
            Assert.Equal(3, b);
            Assert.Equal(3, w);
        }

        [Fact]
        public void MultiDirectionFlip_ShouldUpdateCounts()
        {
            var game = new GameLogic(_ => { }, _ => { });
            game.InitializeGame();

            // Setup a position where black at (3,3) flips two stones.
            // Initial stones: Black=2 ((3,1),(1,3)), White=2 ((3,2),(2,3))
            var bb = TestHelpers.BoardFromStrings(
                "........",
                "...B....",
                "...W....",
                ".BW.....",
                "........",
                "........",
                "........",
                "........");

            game.SetPositionForTest(bb, Stone.BLACK);

            Assert.True(game.IsValidMove(3, 3, Stone.BLACK));
            Assert.True(game.TryPlaceAt(3, 3));

            var (b, w) = game.CountStones();
            Assert.Equal(5, b);
            Assert.Equal(0, w);
            Assert.Equal(Stone.WHITE, game.GetCurrentPlayer());
        }

        [Fact]
        public void ApplyMove_ShouldNotAllowIllegalMoveThenLegalMoveStillWorks()
        {
            var game = TestHelpers.NewGame();

            // Illegal
            Assert.False(game.TryPlaceAt(0, 0));
            Assert.Equal(Stone.BLACK, game.GetCurrentPlayer());

            // Legal
            Assert.True(game.TryPlaceAt(2, 4));
            Assert.Equal(Stone.WHITE, game.GetCurrentPlayer());
        }
    }
}
