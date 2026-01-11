using System;

namespace reversi_cs
{
    /**
     * NN評価関数を用いたアルファベータ探索プレイヤー。
     * 
     * 現時点では UI への組み込みは行わず、探索呼び出し用のクラスとして用意する。
     */
    public sealed class AlphaBetaPlayer
    {
        private readonly Stone stone;
        private readonly AlphaBetaSearch search;

        public AlphaBetaPlayer(Stone stone, NNEvaluator evaluator)
        {
            this.stone = stone;
            this.search = new AlphaBetaSearch(evaluator);
        }

        public Stone Stone => stone;

        /**
         * 指定局面から探索して手を返す。
         * @param pos 局面
         * @param sideToMove 手番
         * @param depth 探索深さ（ply）
         */
        public (int x, int y)? ChooseMove(BitBoard pos, Stone sideToMove, int depth)
        {
            if (sideToMove != stone) return null;
            return search.FindBestMove(pos, sideToMove, depth);
        }
    }
}
