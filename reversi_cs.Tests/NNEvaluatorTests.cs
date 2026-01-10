using System.Collections.Generic;
using Xunit;

namespace reversi_cs.Tests
{
    public class NNEvaluatorTests
    {
        [Fact]
        public void Evaluate_ShouldRespectPerspectiveSign()
        {
            // Model: output = sum(input) (linear 1-layer)
            // Stage: all stages share the same weights here.
            var model = CreateSumModel(inputSize: 64 * 5);
            var eval = new reversi_cs.NNEvaluator(model);

            var pos = BitBoard.Initial;
            var sideToMove = Stone.BLACK;

            double stm = eval.EvaluateSideToMove(pos, sideToMove);
            double asBlack = eval.Evaluate(pos, sideToMove, Stone.BLACK);
            double asWhite = eval.Evaluate(pos, sideToMove, Stone.WHITE);

            Assert.Equal(stm, asBlack);
            Assert.Equal(-stm, asWhite);
        }

        [Fact]
        public void Evaluate_EmptyPosition_ShouldBeDeterministic()
        {
            var model = CreateSumModel(inputSize: 64 * 5);
            var eval = new reversi_cs.NNEvaluator(model);

            // empty board
            var pos = new BitBoard(0UL, 0UL);

            // If empty: for each square, channel[2]=1 (empty) => sum = 64
            double y = eval.EvaluateSideToMove(pos, Stone.BLACK);
            Assert.Equal(64, y, 6);
        }

        private static reversi_cs.NNEvalModel CreateSumModel(int inputSize)
        {
            // W: [65][L=1][out=1][in=inputSize]
            // b: [65][L=1][out=1]
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
