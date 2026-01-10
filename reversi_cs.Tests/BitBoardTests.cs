using System;
using Xunit;

namespace reversi_cs.Tests
{
    public class BitBoardTests
    {
        [Fact]
        public void ToIndex_ShouldMapRowMajor_YTimes8PlusX()
        {
            Assert.Equal(0, BitBoard.ToIndex(0, 0));
            Assert.Equal(1, BitBoard.ToIndex(1, 0));
            Assert.Equal(8, BitBoard.ToIndex(0, 1));
            Assert.Equal(63, BitBoard.ToIndex(7, 7));
        }

        [Fact]
        public void ToIndex_OutOfRange_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BitBoard.ToIndex(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BitBoard.ToIndex(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => BitBoard.ToIndex(8, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BitBoard.ToIndex(0, 8));
        }

        [Fact]
        public void Initial_ShouldMatchExpectedStones()
        {
            var bb = BitBoard.Initial;

            Assert.Equal(2, bb.PopCountBlack());
            Assert.Equal(2, bb.PopCountWhite());

            TestHelpers.AssertStone(bb, 3, 3, Stone.BLACK);
            TestHelpers.AssertStone(bb, 4, 4, Stone.BLACK);
            TestHelpers.AssertStone(bb, 3, 4, Stone.WHITE);
            TestHelpers.AssertStone(bb, 4, 3, Stone.WHITE);
        }

        [Fact]
        public void Initial_LegalMoves_ShouldMatchReversiOpening()
        {
            var bb = BitBoard.Initial;

            ulong blackMoves = bb.GetLegalMovesMask(Stone.BLACK);
            ulong whiteMoves = bb.GetLegalMovesMask(Stone.WHITE);

            Assert.NotEqual(0UL, blackMoves);
            Assert.NotEqual(0UL, whiteMoves);

            Assert.True((blackMoves & BitBoard.ToMask(2, 4)) != 0);
            Assert.True((blackMoves & BitBoard.ToMask(3, 5)) != 0);
            Assert.True((blackMoves & BitBoard.ToMask(4, 2)) != 0);
            Assert.True((blackMoves & BitBoard.ToMask(5, 3)) != 0);

            Assert.True((whiteMoves & BitBoard.ToMask(2, 3)) != 0);
            Assert.True((whiteMoves & BitBoard.ToMask(3, 2)) != 0);
            Assert.True((whiteMoves & BitBoard.ToMask(4, 5)) != 0);
            Assert.True((whiteMoves & BitBoard.ToMask(5, 4)) != 0);
        }

        [Fact]
        public void GetFlipsMask_OnIllegalSquare_ShouldBeZero()
        {
            var bb = BitBoard.Initial;

            Assert.Equal(0UL, bb.GetFlipsMask(Stone.BLACK, 0, 0));
            Assert.Equal(0UL, bb.GetFlipsMask(Stone.WHITE, 0, 0));
        }

        [Fact]
        public void ApplyMove_IllegalMove_ShouldNotChangePosition()
        {
            var bb = BitBoard.Initial;
            var next = bb.ApplyMove(Stone.BLACK, 0, 0);
            Assert.Equal(bb.Black, next.Black);
            Assert.Equal(bb.White, next.White);
        }

        [Fact]
        public void ApplyMove_BlackAt_2_4_ShouldFlip_3_4()
        {
            var bb = BitBoard.Initial;

            ulong flips = bb.GetFlipsMask(Stone.BLACK, 2, 4);
            Assert.True((flips & BitBoard.ToMask(3, 4)) != 0);
            Assert.Equal(1, PopCount(flips));

            var next = bb.ApplyMove(Stone.BLACK, 2, 4);
            Assert.Equal(4, next.PopCountBlack());
            Assert.Equal(1, next.PopCountWhite());
            TestHelpers.AssertStone(next, 2, 4, Stone.BLACK);
            TestHelpers.AssertStone(next, 3, 4, Stone.BLACK);
        }

        private static int PopCount(ulong value)
        {
            int count = 0;
            while (value != 0)
            {
                value &= value - 1;
                count++;
            }
            return count;
        }

        [Fact]
        public void ApplyMove_ShouldPreserveDisjointBlackWhiteMasks()
        {
            var bb = BitBoard.Initial;
            var next = bb.ApplyMove(Stone.BLACK, 2, 4);
            Assert.Equal(0UL, next.Black & next.White);
        }

        [Fact]
        public void ApplyMove_ShouldFlipInMultipleDirections()
        {
            // Move: Black at (3,3) flips (3,2) vertically and (2,3) horizontally.
            //
            // Vertical: (3,3)=move, (3,2)=W, (3,1)=B
            // Horizontal: (3,3)=move, (2,3)=W, (1,3)=B
            var bb = TestHelpers.BoardFromStrings(
                "........",
                "...B....",
                "...W....",
                ".BW.....",
                "........",
                "........",
                "........",
                "........");

            Assert.True(bb.IsLegalMove(Stone.BLACK, 3, 3));

            ulong flips = bb.GetFlipsMask(Stone.BLACK, 3, 3);
            Assert.True((flips & BitBoard.ToMask(3, 2)) != 0);
            Assert.True((flips & BitBoard.ToMask(2, 3)) != 0);

            var next = bb.ApplyMove(Stone.BLACK, 3, 3);
            TestHelpers.AssertStone(next, 3, 3, Stone.BLACK);
            TestHelpers.AssertStone(next, 3, 2, Stone.BLACK);
            TestHelpers.AssertStone(next, 2, 3, Stone.BLACK);
            Assert.Equal(0UL, next.Black & next.White);
        }

        [Fact]
        public void GetLegalMovesMask_WhenNoMoves_ShouldBeZero()
        {
            // All black board -> white has no moves
            var bb = new BitBoard(~0UL, 0UL);
            Assert.Equal(0UL, bb.GetLegalMovesMask(Stone.WHITE));
        }
    }
}
