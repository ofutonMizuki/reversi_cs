using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace reversi_cs
{
    /**
     * TypeScript の NNEval を参考にした NN 評価関数。
     * 
     * - 入力: 64マス x Cチャネル（既定: C=5）
     *   0: 手番側の石
     *   1: 相手側の石
     *   2: 空き
     *   3: 手番側の合法手
     *   4: 相手側の合法手
     * - 段階: 盤面の石数 n = 0..64 で重みを切り替える
     * - 出力: 実数スコア（手番視点）
     */
    public sealed class NNEvaluator
    {
        private readonly int inputChannels;

        // W[n][l][out][in]
        private readonly double[][][][] W;

        // B[n][l][out]
        private readonly double[][][] B;

        /**
         * コンストラクタ。
         * @param model TS 版の save(JSON) を読み込んだモデル
         * @param inputChannels 入力チャネル数（保存済みモデルから決定したい場合は 0 を指定）
         */
        public NNEvaluator(NNEvalModel model, int inputChannels = 0)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));
            if (!string.Equals(model.Type, "NNEval", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid model type", nameof(model));
            if (model.Version != 3)
                throw new ArgumentException("Only version=3 is supported", nameof(model));
            if (model.HiddenSizes is null || model.HiddenSizes.Length == 0)
                throw new ArgumentException("hiddenSizes is required", nameof(model));
            if (model.W is null || model.B is null)
                throw new ArgumentException("W/b is required", nameof(model));

            // Convert nested Lists to arrays for faster access.
            W = model.W.Select(n => n.Select(l => l.Select(o => o.ToArray()).ToArray()).ToArray()).ToArray();
            B = model.B.Select(n => n.Select(l => l.ToArray()).ToArray()).ToArray();

            if (W.Length != 65 || B.Length != 65)
                throw new ArgumentException("W/b must have 65 stages", nameof(model));

            // Determine input channels from first layer width if not specified.
            int inferred = 0;
            try
            {
                inferred = W[0][0][0].Length / 64;
            }
            catch
            {
                inferred = 5;
            }

            this.inputChannels = inputChannels > 0 ? inputChannels : Math.Max(1, inferred);
        }

        /**
         * JSON ファイルからモデルを読み込み、NNEvaluator を生成する。
         * @param path JSON ファイルパス
         */
        public static NNEvaluator LoadFromFile(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            var json = File.ReadAllText(path);
            var model = JsonSerializer.Deserialize<NNEvalModel>(json);
            if (model is null) throw new InvalidDataException("Failed to deserialize model");
            return new NNEvaluator(model);
        }

        /**
         * 手番視点の評価値を返す。
         * @param pos 局面
         * @param sideToMove 手番の色
         */
        public double EvaluateSideToMove(BitBoard pos, Stone sideToMove)
        {
            var (self, opp) = BitBoard.Split(pos, sideToMove);

            int n = pos.PopCountBlack() + pos.PopCountWhite();
            n = Math.Clamp(n, 0, 64);

            ulong selfMoves = pos.GetLegalMovesMask(sideToMove);
            ulong oppMoves = pos.GetLegalMovesMask(Opponent(sideToMove));

            var x = MakeInput(self, opp, selfMoves, oppMoves);
            return Forward(n, x);
        }

        /**
         * 指定視点（color）での評価値を返す。
         * @param pos 局面
         * @param sideToMove 手番
         * @param color 評価値を返す視点の色
         */
        public double Evaluate(BitBoard pos, Stone sideToMove, Stone color)
        {
            var y = EvaluateSideToMove(pos, sideToMove);
            return color == sideToMove ? y : -y;
        }

        private double[] MakeInput(ulong self, ulong opp, ulong selfMoves, ulong oppMoves)
        {
            int C = inputChannels;
            var x = new double[64 * C];

            for (int i = 0; i < 64; i++)
            {
                int baseIdx = i * C;
                bool isSelf = (self & 1UL) != 0;
                bool isOpp = (opp & 1UL) != 0;

                if (isSelf)
                    x[baseIdx + 0] = 1;
                else if (isOpp)
                    x[baseIdx + 1] = 1;
                else
                    x[baseIdx + 2] = 1;

                if (C >= 4 && (selfMoves & 1UL) != 0)
                    x[baseIdx + 3] = 1;
                if (C >= 5 && (oppMoves & 1UL) != 0)
                    x[baseIdx + 4] = 1;

                self >>= 1;
                opp >>= 1;
                selfMoves >>= 1;
                oppMoves >>= 1;
            }

            return x;
        }

        private double Forward(int n, double[] input)
        {
            var a = input;
            int layers = W[n].Length;

            for (int l = 0; l < layers; l++)
            {
                var Wl = W[n][l];
                var bl = B[n][l];

                int outSize = Wl.Length;
                int inSize = a.Length;

                var z = new double[outSize];
                for (int o = 0; o < outSize; o++)
                {
                    double s = bl[o];
                    var row = Wl[o];
                    for (int i = 0; i < inSize; i++)
                        s += row[i] * a[i];

                    z[o] = s;
                }

                // last layer is linear, others tanh
                if (l == layers - 1)
                {
                    a = z;
                }
                else
                {
                    var t = new double[outSize];
                    for (int i = 0; i < outSize; i++)
                        t[i] = Math.Tanh(z[i]);
                    a = t;
                }
            }

            return a.Length > 0 ? a[0] : 0;
        }

        private static Stone Opponent(Stone s) => s == Stone.BLACK ? Stone.WHITE : Stone.BLACK;
    }
}
