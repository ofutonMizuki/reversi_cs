using System;
using System.Threading;
using System.Threading.Tasks;

namespace reversi_cs
{
    /**
     * アルファベータ探索の非同期実行ヘルパ。
     * 
     * UI スレッドをブロックしないために Task.Run でバックグラウンド実行する。
     */
    public sealed class AlphaBetaSearchAsync
    {
        private readonly AlphaBetaSearch search;

        public AlphaBetaSearchAsync(AlphaBetaSearch search)
        {
            this.search = search ?? throw new ArgumentNullException(nameof(search));
        }

        /**
         * バックグラウンドで探索し、最善手を返す。
         * @param pos 局面
         * @param sideToMove 手番
         * @param depth 探索深さ
         * @param cancellationToken 中断用
         * @param timeLimit タイムリミット（未指定なら無制限）
         */
        public Task<(int x, int y)?> FindBestMoveAsync(BitBoard pos, Stone sideToMove, int depth, CancellationToken cancellationToken, TimeSpan? timeLimit = null)
        {
            DateTime? deadlineUtc = timeLimit.HasValue ? DateTime.UtcNow.Add(timeLimit.Value) : null;

            return Task.Run(
                () => search.FindBestMove(pos, sideToMove, depth, cancellationToken, deadlineUtc),
                cancellationToken);
        }

        /**
         * バックグラウンドで反復深化探索し、最善手を返す。
         * @param pos 局面
         * @param sideToMove 手番
         * @param maxDepth 最大探索深さ
         * @param cancellationToken 中断用
         * @param timeLimit タイムリミット（未指定なら無制限）
         */
        public Task<(int x, int y)?> FindBestMoveIterativeAsync(BitBoard pos, Stone sideToMove, int maxDepth, CancellationToken cancellationToken, TimeSpan? timeLimit = null)
        {
            DateTime? deadlineUtc = timeLimit.HasValue ? DateTime.UtcNow.Add(timeLimit.Value) : null;

            return Task.Run(
                () => search.FindBestMoveIterative(pos, sideToMove, maxDepth, cancellationToken, deadlineUtc),
                cancellationToken);
        }
    }
}
