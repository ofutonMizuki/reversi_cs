using System;
using System.Collections.Generic;
using Xunit;

namespace reversi_cs.Tests
{
    public class TranspositionTableTests
    {
        [Fact]
        public void Store_ThenTryGet_ShouldHit()
        {
            var tt = new reversi_cs.TranspositionTable(1 << 10);
            var key = reversi_cs.TranspositionTable.ComputeKey(BitBoard.Initial, Stone.BLACK, depth: 4);

            tt.Store(key, 123.0, depth: 4, reversi_cs.TranspositionTable.Bound.Exact);

            Assert.True(tt.TryGet(key, out var entry));
            Assert.Equal(123.0, entry.Value);
            Assert.Equal(4, entry.Depth);
            Assert.Equal(reversi_cs.TranspositionTable.Bound.Exact, entry.Bound);
        }

        [Fact]
        public void Store_WithBestMove_ShouldRoundTrip()
        {
            var tt = new reversi_cs.TranspositionTable(1 << 10);
            var key = reversi_cs.TranspositionTable.ComputeKey(BitBoard.Initial, Stone.BLACK, depth: 4);

            tt.Store(key, 1.0, depth: 4, reversi_cs.TranspositionTable.Bound.Exact, bestMove: (2, 3));

            Assert.True(tt.TryGet(key, out var entry));
            Assert.Equal((2, 3), entry.DecodeBestMove());
        }

        [Fact]
        public void AlphaBetaSearch_WithTranspositionTable_ShouldReturnLegalMove()
        {
            var eval = new reversi_cs.NNEvaluator(CreateSumModel(inputSize: 64 * 5));
            var tt = new reversi_cs.TranspositionTable(1 << 12);
            var search = new reversi_cs.AlphaBetaSearch(eval, tt);

            var pos = BitBoard.Initial;
            var move = search.FindBestMove(pos, Stone.BLACK, depth: 3);

            Assert.NotNull(move);
            Assert.True(pos.IsLegalMove(Stone.BLACK, move!.Value.x, move.Value.y));
        }

        private static reversi_cs.NNEvalModel CreateSumModel(int inputSize)
        {
            var W = new List<List<List<List<double>>>>(65);
            var B = new List<List<List<double>>>(65);

            for (int n = 0; n < 65; n++)
            {
                var layerW = new List<List<List<double>>>(1);
                var layerB = new List<List<double>>(1);

                var outRow = new List<List<double>>(1);
                var weights = new List<double>(inputSize);
                for (int i = 0; i < inputSize; i++) weights.Add(1.0);
                outRow.Add(weights);
                layerW.Add(outRow);

                layerB.Add(new List<double> { 0.0 });

                W.Add(layerW);
                B.Add(layerB);
            }

            return new reversi_cs.NNEvalModel
            {
                Type = "NNEval",
                Version = 3,
                HiddenSizes = new[] { 1 },
                W = W,
                B = B
            };
        }
    }
}
