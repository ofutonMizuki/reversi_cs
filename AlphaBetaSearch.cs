using System;
using System.Threading;

namespace reversi_cs
{
    /**
     * アルファベータ法（Negamax 形式）の探索。
     * 
     * - 評価値は「手番視点」になるように統一する。
     * - 合法手なしの場合はパス（手番交代）
     * - 両者合法手なしで終局
     */
    public sealed class AlphaBetaSearch
    {
        private readonly NNEvaluator evaluator;

        public AlphaBetaSearch(NNEvaluator evaluator)
        {
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        /**
         * 指定局面から最善手を返す。
         * @param pos 局面
         * @param sideToMove 手番
         * @param depth 探索深さ（ply）
         * @param cancellationToken 途中中断用
         * @param deadlineUtc 探索の締切（UTC）。未指定なら無制限
         * @return 最善手。合法手が無ければ null（パス）
         */
        public (int x, int y)? FindBestMove(BitBoard pos, Stone sideToMove, int depth, CancellationToken cancellationToken = default, DateTime? deadlineUtc = null)
        {
            if (depth < 0) throw new ArgumentOutOfRangeException(nameof(depth));

            ulong movesMask = pos.GetLegalMovesMask(sideToMove);
            if (movesMask == 0) return null;

            double best = double.NegativeInfinity;
            (int x, int y)? bestMove = null;

            foreach (var (x, y) in pos.EnumerateLegalMoves(sideToMove))
            {
                ThrowIfCancelled(cancellationToken, deadlineUtc);

                var next = pos.ApplyMove(sideToMove, x, y);
                double score = -Negamax(next, Opponent(sideToMove), depth - 1, double.NegativeInfinity, double.PositiveInfinity, cancellationToken, deadlineUtc);

                if (score > best)
                {
                    best = score;
                    bestMove = (x, y);
                }
            }

            return bestMove;
        }

        private double Negamax(BitBoard pos, Stone sideToMove, int depth, double alpha, double beta, CancellationToken cancellationToken, DateTime? deadlineUtc)
        {
            ThrowIfCancelled(cancellationToken, deadlineUtc);

            if (depth == 0 || pos.IsTerminal())
                return EvaluateTerminalAware(pos, sideToMove);

            ulong movesMask = pos.GetLegalMovesMask(sideToMove);

            // Pass
            if (movesMask == 0)
            {
                return -Negamax(pos, Opponent(sideToMove), depth - 1, -beta, -alpha, cancellationToken, deadlineUtc);
            }

            double best = double.NegativeInfinity;

            foreach (var (x, y) in pos.EnumerateLegalMoves(sideToMove))
            {
                ThrowIfCancelled(cancellationToken, deadlineUtc);

                var next = pos.ApplyMove(sideToMove, x, y);
                double score = -Negamax(next, Opponent(sideToMove), depth - 1, -beta, -alpha, cancellationToken, deadlineUtc);

                if (score > best) best = score;
                if (score > alpha) alpha = score;
                if (alpha >= beta) break;
            }

            return best;
        }

        private double EvaluateTerminalAware(BitBoard pos, Stone sideToMove)
        {
            if (!pos.IsTerminal())
                return evaluator.EvaluateSideToMove(pos, sideToMove);

            // 終局時は石差を強く反映（勝敗が（ほぼ）確定）
            // 手番視点に揃える
            int diff = pos.StoneDiff(); // black-white
            int signed = sideToMove == Stone.BLACK ? diff : -diff;

            // NN のスコアスケールが不明なので、石差を大きめの値で返す。
            // これにより終局近辺で勝ちを優先しやすくする。
            return signed * 1000.0;
        }

        private static void ThrowIfCancelled(CancellationToken token, DateTime? deadlineUtc)
        {
            token.ThrowIfCancellationRequested();
            if (deadlineUtc.HasValue && DateTime.UtcNow >= deadlineUtc.Value)
                throw new OperationCanceledException("Search timed out", token);
        }

        private static Stone Opponent(Stone s) => s == Stone.BLACK ? Stone.WHITE : Stone.BLACK;
    }
}
