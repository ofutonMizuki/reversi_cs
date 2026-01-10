using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace reversi_cs.Tests
{
    internal static class TestHelpers
    {
        public static reversi_cs.GameLogic NewGame()
        {
            var game = new reversi_cs.GameLogic(_ => { }, _ => { });
            game.InitializeGame();
            return game;
        }

        public static HashSet<(int x, int y)> ToSet(IEnumerable<(int x, int y)> moves)
            => moves.ToHashSet();

        public static void AssertMovesEqual(IEnumerable<(int x, int y)> actual, params (int x, int y)[] expected)
        {
            var set = actual.ToHashSet();
            Assert.Equal(expected.Length, set.Count);
            foreach (var e in expected)
                Assert.Contains(e, set);
        }

        public static void AssertStone(BitBoard bb, int x, int y, Stone expected)
        {
            Assert.Equal(expected, bb.GetStoneAt(x, y));
        }

        public static BitBoard BoardFromStrings(params string[] rowsTopToBottom)
        {
            if (rowsTopToBottom.Length != 8) throw new ArgumentException("rows must be 8", nameof(rowsTopToBottom));

            ulong black = 0;
            ulong white = 0;

            for (int y = 0; y < 8; y++)
            {
                string row = rowsTopToBottom[y];
                if (row.Length != 8) throw new ArgumentException("each row must be 8", nameof(rowsTopToBottom));

                for (int x = 0; x < 8; x++)
                {
                    char c = row[x];
                    ulong m = BitBoard.ToMask(x, y);
                    if (c == 'B') black |= m;
                    else if (c == 'W') white |= m;
                    else if (c == '.') { }
                    else throw new ArgumentException("use 'B','W','.'", nameof(rowsTopToBottom));
                }
            }

            return new BitBoard(black, white);
        }
    }
}
