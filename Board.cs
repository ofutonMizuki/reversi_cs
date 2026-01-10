namespace reversi_cs
{
    /**
     * ボードを表すクラス。8x8 の石の配置を保持する。
     * 
     * 内部表現は BitBoard（黒/白のビット列）とし、UI 互換のために従来の API を提供する。
     */
    public sealed class Board
    {
        private BitBoard bitBoard;

        /**
         * コンストラクタ。初期配置を設定する。
         */
        public Board()
        {
            bitBoard = BitBoard.Initial;
        }

        /**
         * ボードを初期状態に戻す。中央に4つの石を配置する。
         */
        public void Init()
        {
            bitBoard = BitBoard.Initial;
        }

        /**
         * 指定座標の色を返す。
         * @param x X座標（0-7）
         * @param y Y座標（0-7）
         * @return 指定座標の石の色
         */
        public Stone GetPositionColor(int x, int y)
        {
            return bitBoard.GetStoneAt(x, y);
        }

        /**
         * 指定座標に色を設定する。
         * @param x X座標（0-7）
         * @param y Y座標（0-7）
         * @param s 設定する色
         */
        public void SetPositionColor(int x, int y, Stone s)
        {
            ulong mask = BitBoard.ToMask(x, y);

            ulong black = bitBoard.Black;
            ulong white = bitBoard.White;

            black &= ~mask;
            white &= ~mask;

            if (s == Stone.BLACK) black |= mask;
            else if (s == Stone.WHITE) white |= mask;

            bitBoard = new BitBoard(black, white);
        }

        /**
         * 座標がボード内にあるかチェックする。
         * @param x X座標
         * @param y Y座標
         * @return ボード内なら true
         */
        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        /**
         * 内部の BitBoard を取得する。
         */
        public BitBoard GetBitBoard() => bitBoard;

        /**
         * 内部の BitBoard を置き換える。
         */
        public void SetBitBoard(BitBoard bb) => bitBoard = bb;
    }
}
