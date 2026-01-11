using System;

namespace reversi_cs
{
    /**
     * 8x8 盤面を 64bit のビット列で表すビットボード。
     * 
     * - 1 ビットが 1 マスに対応する。
     * - ビットの割り当ては index = y*8 + x（x:0-7, y:0-7）とする。
     * - Black/White それぞれの石配置を ulong で保持する。
     * 
     * 将来的な高速化（合法手生成・反転処理など）のために導入している。
     */
    public readonly struct BitBoard
    {
        /** 黒石の配置を表すビット列 */
        public readonly ulong Black;

        /** 白石の配置を表すビット列 */
        public readonly ulong White;

        /**
         * コンストラクタ。
         * @param black 黒石ビット列
         * @param white 白石ビット列
         */
        public BitBoard(ulong black, ulong white)
        {
            Black = black;
            White = white;
        }

        /**
         * 初期局面のビットボードを返す。
         * Board.Init() と同一の配置になるように設定する。
         */
        public static BitBoard Initial
        {
            get
            {
                // Initial placement matches Board.Init():
                // (3,3)=BLACK, (3,4)=WHITE, (4,3)=WHITE, (4,4)=BLACK
                ulong black = 0;
                ulong white = 0;

                black |= 1UL << ToIndex(3, 3);
                black |= 1UL << ToIndex(4, 4);

                white |= 1UL << ToIndex(3, 4);
                white |= 1UL << ToIndex(4, 3);

                return new BitBoard(black, white);
            }
        }

        /**
         * (x, y) をビットインデックス（0-63）に変換する。
         * @param x X座標（0-7）
         * @param y Y座標（0-7）
         */
        public static int ToIndex(int x, int y)
        {
            if ((uint)x >= 8u) throw new ArgumentOutOfRangeException(nameof(x));
            if ((uint)y >= 8u) throw new ArgumentOutOfRangeException(nameof(y));
            return (y << 3) | x;
        }

        /**
         * (x, y) に対応する 1 ビットマスクを返す。
         */
        public static ulong ToMask(int x, int y) => 1UL << ToIndex(x, y);

        /**
         * 指定マスが埋まっているかを返す。
         */
        public bool IsOccupied(int x, int y)
        {
            var m = ToMask(x, y);
            return ((Black | White) & m) != 0;
        }

        /**
         * 指定マスの石を返す。
         */
        public Stone GetStoneAt(int x, int y)
        {
            var m = ToMask(x, y);
            if ((Black & m) != 0) return Stone.BLACK;
            if ((White & m) != 0) return Stone.WHITE;
            return Stone.EMPTY;
        }

        /**
         * player 視点で (自分の石, 相手の石) のビット列に分割する。
         */
        public static (ulong self, ulong opp) Split(BitBoard bb, Stone player)
            => player == Stone.BLACK ? (bb.Black, bb.White) : (bb.White, bb.Black);

        /**
         * player 視点の (自分の石, 相手の石) を BitBoard に戻す。
         */
        public static BitBoard Merge(ulong self, ulong opp, Stone player)
            => player == Stone.BLACK ? new BitBoard(self, opp) : new BitBoard(opp, self);

        /** 黒石数を数える */
        public int PopCountBlack() => BitOperationsCompat.PopCount(Black);

        /** 白石数を数える */
        public int PopCountWhite() => BitOperationsCompat.PopCount(White);

        /** 全ての石（黒白合成）のビット列 */
        public ulong Occupied => Black | White;

        /** 空きマスのビット列（上位ビットも立つので利用側で盤面内に限定する） */
        public ulong Empty => ~Occupied;

        private const ulong FileA = 0x0101010101010101UL;
        private const ulong FileH = 0x8080808080808080UL;

        private static ulong ShiftE(ulong b) => (b & ~FileH) << 1;
        private static ulong ShiftW(ulong b) => (b & ~FileA) >> 1;
        private static ulong ShiftN(ulong b) => b >> 8;
        private static ulong ShiftS(ulong b) => b << 8;
        private static ulong ShiftNE(ulong b) => (b & ~FileH) >> 7;
        private static ulong ShiftNW(ulong b) => (b & ~FileA) >> 9;
        private static ulong ShiftSE(ulong b) => (b & ~FileH) << 9;
        private static ulong ShiftSW(ulong b) => (b & ~FileA) << 7;

        /**
         * 指定プレイヤーの合法手をビットマスクで返す。
         */
        public ulong GetLegalMovesMask(Stone player)
        {
            var (self, opp) = Split(this, player);
            return GetLegalMovesMask(self, opp);
        }

        /**
         * 指定プレイヤー視点（self/opp）で合法手をビットマスクで返す。
         */
        public static ulong GetLegalMovesMask(ulong self, ulong opp)
        {
            ulong empty = ~(self | opp);
            ulong moves = 0;

            moves |= LegalMovesDir(self, opp, empty, ShiftE);
            moves |= LegalMovesDir(self, opp, empty, ShiftW);
            moves |= LegalMovesDir(self, opp, empty, ShiftN);
            moves |= LegalMovesDir(self, opp, empty, ShiftS);
            moves |= LegalMovesDir(self, opp, empty, ShiftNE);
            moves |= LegalMovesDir(self, opp, empty, ShiftNW);
            moves |= LegalMovesDir(self, opp, empty, ShiftSE);
            moves |= LegalMovesDir(self, opp, empty, ShiftSW);

            return moves;
        }

        private static ulong LegalMovesDir(ulong self, ulong opp, ulong empty, Func<ulong, ulong> shift)
        {
            ulong mask = shift(self) & opp;
            for (int i = 0; i < 5; i++) mask |= shift(mask) & opp;
            return shift(mask) & empty;
        }

        /**
         * 指定マスへの着手で裏返る石（相手石）のビットマスクを返す。
         */
        public ulong GetFlipsMask(Stone player, int x, int y)
        {
            ulong move = ToMask(x, y);
            if ((Occupied & move) != 0) return 0;

            var (self, opp) = Split(this, player);
            ulong flips = 0;

            flips |= FlipsDir(move, self, opp, ShiftE);
            flips |= FlipsDir(move, self, opp, ShiftW);
            flips |= FlipsDir(move, self, opp, ShiftN);
            flips |= FlipsDir(move, self, opp, ShiftS);
            flips |= FlipsDir(move, self, opp, ShiftNE);
            flips |= FlipsDir(move, self, opp, ShiftNW);
            flips |= FlipsDir(move, self, opp, ShiftSE);
            flips |= FlipsDir(move, self, opp, ShiftSW);

            return flips;
        }

        private static ulong FlipsDir(ulong move, ulong self, ulong opp, Func<ulong, ulong> shift)
        {
            ulong cur = shift(move);
            ulong flips = 0;
            while (cur != 0 && (cur & opp) != 0)
            {
                flips |= cur;
                cur = shift(cur);
            }

            if (cur != 0 && (cur & self) != 0) return flips;
            return 0;
        }

        /**
         * 指定マスが合法手かを返す。
         */
        public bool IsLegalMove(Stone player, int x, int y)
        {
            ulong m = ToMask(x, y);
            return (GetLegalMovesMask(player) & m) != 0;
        }

        /**
         * 合法手なら適用後の BitBoard を返す。
         * 不正な手の場合は現状のまま返す。
         */
        public BitBoard ApplyMove(Stone player, int x, int y)
        {
            ulong move = ToMask(x, y);
            var (self, opp) = Split(this, player);
            if (((self | opp) & move) != 0) return this;

            ulong flips = 0;
            flips |= FlipsDir(move, self, opp, ShiftE);
            flips |= FlipsDir(move, self, opp, ShiftW);
            flips |= FlipsDir(move, self, opp, ShiftN);
            flips |= FlipsDir(move, self, opp, ShiftS);
            flips |= FlipsDir(move, self, opp, ShiftNE);
            flips |= FlipsDir(move, self, opp, ShiftNW);
            flips |= FlipsDir(move, self, opp, ShiftSE);
            flips |= FlipsDir(move, self, opp, ShiftSW);

            if (flips == 0) return this;

            self ^= flips;
            opp ^= flips;
            self |= move;

            return Merge(self, opp, player);
        }

        /**
         * 指定プレイヤーの合法手を (x,y) の列挙として返す。
         */
        public System.Collections.Generic.IEnumerable<(int x, int y)> EnumerateLegalMoves(Stone player)
        {
            ulong mask = GetLegalMovesMask(player);
            while (mask != 0)
            {
                ulong lsb = mask & (ulong)-(long)mask;
                int idx = TrailingZeroCount(lsb);
                yield return (idx & 7, idx >> 3);
                mask ^= lsb;
            }
        }

        /**
         * 両者とも合法手が無い場合に終局とみなす。
         */
        public bool IsTerminal()
        {
            return GetLegalMovesMask(Stone.BLACK) == 0 && GetLegalMovesMask(Stone.WHITE) == 0;
        }

        /**
         * 石数差（black - white）を返す。
         */
        public int StoneDiff()
        {
            return PopCountBlack() - PopCountWhite();
        }

        private static int TrailingZeroCount(ulong bit)
        {
#if NET7_0_OR_GREATER
            return System.Numerics.BitOperations.TrailingZeroCount(bit);
#else
            int idx = 0;
            while ((bit & 1UL) == 0)
            {
                bit >>= 1;
                idx++;
            }
            return idx;
#endif
        }
    }

    /**
     * BitOperations を利用できないターゲット向けの互換実装（.NET 10 では基本的に BitOperations を使用する）。
     */
    internal static class BitOperationsCompat
    {
        public static int PopCount(ulong value)
        {
#if NET7_0_OR_GREATER
            return System.Numerics.BitOperations.PopCount(value);
#else
            // SW fallback (should not be used on .NET 10, but keeps the file standalone).
            int count = 0;
            while (value != 0)
            {
                value &= value - 1;
                count++;
            }
            return count;
#endif
        }
    }
}
