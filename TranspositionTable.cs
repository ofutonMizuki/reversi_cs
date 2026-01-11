using System;

namespace reversi_cs
{
    /**
     * アルファベータ探索用の置換テーブル。
     *
     * - キーは局面（黑/白ビットボード）+ 手番 + 残り深さを含める。
     * - 同一キーに対して、探索済みの評価値と境界種別（Exact/Lower/Upper）を保持する。
     * - 衝突時は「より深く探索した結果」を優先して上書きする。
     * - 最善手（PV で得られた手）を保存し、次回探索のムーブオーダリングに利用する。
     */
    public sealed class TranspositionTable
    {
        public enum Bound : byte
        {
            Exact = 0,
            Lower = 1,
            Upper = 2
        }

        public readonly struct Entry
        {
            public readonly ulong Key;
            public readonly double Value;
            public readonly int Depth;
            public readonly Bound Bound;
            public readonly ushort BestMove;

            public Entry(ulong key, double value, int depth, Bound bound, ushort bestMove)
            {
                Key = key;
                Value = value;
                Depth = depth;
                Bound = bound;
                BestMove = bestMove;
            }

            public (int x, int y)? DecodeBestMove()
            {
                if (BestMove == 0) return null;
                int code = BestMove - 1;
                return (code & 7, code >> 3);
            }
        }

        private readonly Entry[] table;
        private readonly ulong mask;

        /**
         * 置換テーブルを作成する。
         * @param sizePowerOfTwo エントリ数（2の冪）。例: 2^20 = 1,048,576
         */
        public TranspositionTable(int sizePowerOfTwo)
        {
            if (sizePowerOfTwo <= 0) throw new ArgumentOutOfRangeException(nameof(sizePowerOfTwo));
            if ((sizePowerOfTwo & (sizePowerOfTwo - 1)) != 0) throw new ArgumentException("sizePowerOfTwo must be power of two", nameof(sizePowerOfTwo));

            table = new Entry[sizePowerOfTwo];
            mask = (ulong)(sizePowerOfTwo - 1);
        }

        /**
         * 既定サイズ（約 1M エントリ）で作成する。
         */
        public static TranspositionTable CreateDefault() => new TranspositionTable(1 << 20);

        /**
         * キーを計算する。
         * @param pos 局面
         * @param sideToMove 手番
         * @param depth 残り深さ
         */
        public static ulong ComputeKey(BitBoard pos, Stone sideToMove, int depth)
        {
            // Zobrist を持っていないため、簡易ハッシュで代用する。
            // 置換テーブル用途なので「同一局面 + 同一手番 + 同一深さ」で安定することが重要。
            unchecked
            {
                ulong x = 1469598103934665603UL;
                x ^= pos.Black;
                x *= 1099511628211UL;
                x ^= pos.White;
                x *= 1099511628211UL;
                x ^= (ulong)sideToMove;
                x *= 1099511628211UL;
                x ^= (ulong)depth;
                x *= 1099511628211UL;
                return x;
            }
        }

        /**
         * ムーブオーダリング用に、深さを無視したキーを計算する。
         * @param pos 局面
         * @param sideToMove 手番
         */
        public static ulong ComputeKeyForMove(BitBoard pos, Stone sideToMove)
        {
            unchecked
            {
                ulong x = 1469598103934665603UL;
                x ^= pos.Black;
                x *= 1099511628211UL;
                x ^= pos.White;
                x *= 1099511628211UL;
                x ^= (ulong)sideToMove;
                x *= 1099511628211UL;
                return x;
            }
        }

        /**
         * エントリを取得する。
         * @param key 検索キー
         * @return ヒットした場合は true
         */
        public bool TryGet(ulong key, out Entry entry)
        {
            var cur = table[(int)(key & mask)];
            if (cur.Key == key && cur.Depth != 0)
            {
                entry = cur;
                return true;
            }

            entry = default;
            return false;
        }

        /**
         * キーが異なる可能性がある前提で、同一スロットのベストムーブだけを取り出す。
         * @param key スロット選択用キー
         */
        public (int x, int y)? TryGetAnyBestMove(ulong key)
        {
            var cur = table[(int)(key & mask)];
            return cur.DecodeBestMove();
        }

        /**
         * エントリを保存する。
         * @param key 検索キー
         * @param value 評価値
         * @param depth 残り深さ
         * @param bound 境界種別
         * @param bestMove 最善手（ムーブオーダリング用、未指定なら null）
         */
        public void Store(ulong key, double value, int depth, Bound bound, (int x, int y)? bestMove = null)
        {
            int idx = (int)(key & mask);
            var cur = table[idx];

            // 初回 or より深い結果を優先
            if (cur.Depth == 0 || cur.Key != key || depth >= cur.Depth)
                table[idx] = new Entry(key, value, depth, bound, EncodeMove(bestMove));
        }

        private static ushort EncodeMove((int x, int y)? move)
        {
            if (!move.HasValue) return 0;
            int x = move.Value.x;
            int y = move.Value.y;
            if ((uint)x >= 8u || (uint)y >= 8u) return 0;
            return (ushort)(1 + ((y << 3) | x));
        }

        /**
         * テーブルをクリアする。
         */
        public void Clear()
        {
            Array.Clear(table, 0, table.Length);
        }
    }
}
