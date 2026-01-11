using System.Collections.Generic;
using Xunit;

namespace reversi_cs.Tests
{
    public class AlphaBetaSearchTests
    {
        [Fact]
        public void FindBestMove_ShouldReturnALegalMove_WhenMovesExist()
        {
            var eval = new reversi_cs.NNEvaluator(CreateSumModel(inputSize: 64 * 5));
            var search = new reversi_cs.AlphaBetaSearch(eval);

            var pos = BitBoard.Initial;
            var move = search.FindBestMove(pos, Stone.BLACK, depth: 2);

            Assert.NotNull(move);
            Assert.True(pos.IsLegalMove(Stone.BLACK, move!.Value.x, move.Value.y));
        }

        [Fact]
        public void FindBestMove_WhenNoMoves_ShouldReturnNull()
        {
            var eval = new reversi_cs.NNEvaluator(CreateSumModel(inputSize: 64 * 5));
            var search = new reversi_cs.AlphaBetaSearch(eval);

            // White has no moves.
            var pos = new BitBoard(~0UL, 0UL);
            var move = search.FindBestMove(pos, Stone.WHITE, depth: 3);
            Assert.Null(move);
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
