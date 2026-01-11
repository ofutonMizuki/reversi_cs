using System;
using System.Collections.Generic;
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
        private readonly TranspositionTable? tt;

        public AlphaBetaSearch(NNEvaluator evaluator)
            : this(evaluator, null)
        {
        }

        /**
         * 置換テーブル付きで探索器を作成する。
         * @param evaluator 評価関数
         * @param transpositionTable 置換テーブル（未使用なら null）
         */
        public AlphaBetaSearch(NNEvaluator evaluator, TranspositionTable? transpositionTable)
        {
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            tt = transpositionTable;
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

        /**
         * 反復深化で最善手を返す。
         * 
         * - 深さ 1..maxDepth で逐次探索し、最後に得られた最善手を返す。
         * - 途中でキャンセルされた場合は、直前に確定した最善手（存在すれば）を返す。
         * @param pos 局面
         * @param sideToMove 手番
         * @param maxDepth 最大探索深さ
         * @param cancellationToken 途中中断用
         * @param deadlineUtc 探索の締切（UTC）。未指定なら無制限
         */
        public (int x, int y)? FindBestMoveIterative(BitBoard pos, Stone sideToMove, int maxDepth, CancellationToken cancellationToken = default, DateTime? deadlineUtc = null)
        {
            if (maxDepth < 0) throw new ArgumentOutOfRangeException(nameof(maxDepth));

            ulong movesMask = pos.GetLegalMovesMask(sideToMove);
            if (movesMask == 0) return null;

            (int x, int y)? bestMove = null;

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                try
                {
                    bestMove = FindBestMoveWithRootOrdering(pos, sideToMove, depth, cancellationToken, deadlineUtc);
                }
                catch (OperationCanceledException)
                {
                    return bestMove;
                }
            }

            return bestMove;
        }

        private (int x, int y)? FindBestMoveWithRootOrdering(BitBoard pos, Stone sideToMove, int depth, CancellationToken cancellationToken, DateTime? deadlineUtc)
        {
            ulong movesMask = pos.GetLegalMovesMask(sideToMove);
            if (movesMask == 0) return null;

            var moves = new List<(int x, int y)>(32);
            foreach (var m in pos.EnumerateLegalMoves(sideToMove)) moves.Add(m);

            MoveOrderInPlace(pos, sideToMove, moves);

            double best = double.NegativeInfinity;
            (int x, int y)? bestMove = null;

            foreach (var (x, y) in moves)
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

            if (tt != null)
            {
                ulong key = TranspositionTable.ComputeKey(pos, sideToMove, depth);
                tt.Store(key, best, depth, TranspositionTable.Bound.Exact, bestMove);
            }

            return bestMove;
        }

        private double Negamax(BitBoard pos, Stone sideToMove, int depth, double alpha, double beta, CancellationToken cancellationToken, DateTime? deadlineUtc)
        {
            ThrowIfCancelled(cancellationToken, deadlineUtc);

            double alphaOrig = alpha;
            double betaOrig = beta;

            if (tt != null)
            {
                ulong key = TranspositionTable.ComputeKey(pos, sideToMove, depth);
                if (tt.TryGet(key, out var entry) && entry.Depth >= depth)
                {
                    if (entry.Bound == TranspositionTable.Bound.Exact) return entry.Value;
                    if (entry.Bound == TranspositionTable.Bound.Lower) alpha = Math.Max(alpha, entry.Value);
                    else if (entry.Bound == TranspositionTable.Bound.Upper) beta = Math.Min(beta, entry.Value);
                    if (alpha >= beta) return entry.Value;
                }
            }

            if (depth == 0 || pos.IsTerminal())
            {
                double leaf = EvaluateTerminalAware(pos, sideToMove);
                if (tt != null)
                {
                    ulong key = TranspositionTable.ComputeKey(pos, sideToMove, depth);
                    tt.Store(key, leaf, depth, TranspositionTable.Bound.Exact);
                }
                return leaf;
            }

            ulong movesMask = pos.GetLegalMovesMask(sideToMove);

            // Pass
            if (movesMask == 0)
            {
                double passScore = -Negamax(pos, Opponent(sideToMove), depth - 1, -beta, -alpha, cancellationToken, deadlineUtc);
                if (tt != null)
                {
                    ulong key = TranspositionTable.ComputeKey(pos, sideToMove, depth);
                    tt.Store(key, passScore, depth, TranspositionTable.Bound.Exact);
                }
                return passScore;
            }

            var moves = new List<(int x, int y)>(32);
            foreach (var m in pos.EnumerateLegalMoves(sideToMove)) moves.Add(m);

            MoveOrderInPlace(pos, sideToMove, moves);

            double best = double.NegativeInfinity;
            (int x, int y)? bestMove = null;

            foreach (var (x, y) in moves)
            {
                ThrowIfCancelled(cancellationToken, deadlineUtc);

                var next = pos.ApplyMove(sideToMove, x, y);
                double score = -Negamax(next, Opponent(sideToMove), depth - 1, -beta, -alpha, cancellationToken, deadlineUtc);

                if (score > best)
                {
                    best = score;
                    bestMove = (x, y);
                }

                if (score > alpha) alpha = score;
                if (alpha >= beta) break;
            }

            if (tt != null)
            {
                ulong key = TranspositionTable.ComputeKey(pos, sideToMove, depth);
                var bound = TranspositionTable.Bound.Exact;
                if (best <= alphaOrig) bound = TranspositionTable.Bound.Upper;
                else if (best >= betaOrig) bound = TranspositionTable.Bound.Lower;

                tt.Store(key, best, depth, bound, bestMove);
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

        private void MoveOrderInPlace(BitBoard pos, Stone sideToMove, List<(int x, int y)> moves)
        {
            if (moves.Count <= 1) return;

            (int x, int y)? ttMove = null;
            if (tt != null)
            {
                // 深さは問わず、同一スロットに入っているベストムーブを優先する。
                ulong key = TranspositionTable.ComputeKeyForMove(pos, sideToMove);
                ttMove = tt.TryGetAnyBestMove(key);
            }

            if (!ttMove.HasValue) return;

            int bestIdx = -1;
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].x == ttMove.Value.x && moves[i].y == ttMove.Value.y)
                {
                    bestIdx = i;
                    break;
                }
            }

            if (bestIdx <= 0) return;

            var tmp = moves[0];
            moves[0] = moves[bestIdx];
            moves[bestIdx] = tmp;
        }
    }
}
