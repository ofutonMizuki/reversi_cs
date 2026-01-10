using System;
using System.Collections.Generic;
using System.Text;

namespace reversi_cs
{
    /**
     * 石の状態を表す列挙型。
     * EMPTY: 空, BLACK: 黒, WHITE: 白
     */
    public enum Stone
    {
        EMPTY,
        BLACK,
        WHITE
    }

    // Board は `Board.cs` に移動

    /**
     * ゲームのロジックを管理するクラス。
     * 描画コールバックに Board を渡すことができる。
     */
    public class GameLogic
    {
        // ゲームのボード（UI 互換のためのコンテナ）
        private Board Board;

        /**
         * ゲーム状態の本体（現在局面）を保持するビットボード。
         */
        private BitBoard bitBoard;

        // ボード描画を依頼するコールバック
        readonly Action<Board> DrawBoard;
        // メッセージ表示用のコールバック（パスやゲーム終了通知）
        readonly Action<string> Notify;
        // 現在のプレイヤー
        private Stone CurrentPlayer = Stone.BLACK;
        private bool isGameOver;

        public Stone GetCurrentPlayer() => this.CurrentPlayer;

        public bool IsGameOver() => isGameOver;

        public List<(int x, int y)> GetValidMoves(Stone player)
        {
            var moves = new List<(int x, int y)>();

            ulong mask = bitBoard.GetLegalMovesMask(player);
            while (mask != 0)
            {
                ulong lsb = mask & (ulong)-(long)mask;
                int idx = BitIndexFromSingleBit(lsb);
                moves.Add((idx & 7, idx >> 3));
                mask ^= lsb;
            }

            return moves;
        }

        public (int x, int y)? GetRandomValidMove(Stone player, Random rng)
        {
            if (rng is null) throw new ArgumentNullException(nameof(rng));
            var moves = GetValidMoves(player);
            if (moves.Count == 0) return null;
            return moves[rng.Next(moves.Count)];
        }

        private static int BitIndexFromSingleBit(ulong bit)
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

        /**
         * コンストラクタ。
         * @param drawBoard 描画用コールバック（Board を受け取る）
         * @param notify 通知用コールバック（文字列を受け取る）
         */
        public GameLogic(Action<Board> drawBoard, Action<string> notify)
        {
            this.Board = new Board();
            this.DrawBoard = drawBoard;
            this.Notify = notify;

            this.bitBoard = BitBoard.Initial;
            this.Board.SetBitBoard(bitBoard);
        }

        /**
         * ゲームを初期化する。ボードを作り直して先手を黒に設定する。
         */
        public void InitializeGame()
        {
            this.Board = new Board();
            this.CurrentPlayer = Stone.BLACK; // 黒から開始
            this.isGameOver = false;

            this.bitBoard = BitBoard.Initial;
            this.Board.SetBitBoard(bitBoard);
            this.DrawBoard(this.Board); // 初期状態を描画
        }

        void NotifyGameOverIfNeeded()
        {
            if (isGameOver) return;
            isGameOver = true;

            var (b, w) = CountStones();
            var winner = b > w ? "Black" : (w > b ? "White" : "Draw");
            this.Notify?.Invoke($"Game Over\nBlack: {b} White: {w}\nWinner: {winner}");
        }

        /**
         * 現在のボードを取得する。
         * @return Board オブジェクト
         */
        public Board GetBoard() => this.Board;

        /**
         * 指定位置が指定プレイヤーにとって合法手か判定する。
         * @param x X座標
         * @param y Y座標
         * @param player 判定対象のプレイヤー
         * @return 合法手であれば true
         */
        public bool IsValidMove(int x, int y, Stone player)
        {
            if (!Board.InBounds(x, y)) return false;
            return bitBoard.IsLegalMove(player, x, y);
        }

        /**
         * 指定プレイヤーが一つでも合法手を持っているか調べる。
         * @param player 調べるプレイヤー
         * @return 合法手があれば true
         */
        public bool HasAnyValidMove(Stone player)
        {
            return bitBoard.GetLegalMovesMask(player) != 0;
        }

        /**
         * 指定位置に石を置こうと試みる。合法なら石を置き裏返し、ターンを進める。
         * パスやゲーム終了時には Notify を通してメッセージを通知する。
         * @param x X座標
         * @param y Y座標
         * @return 石を置けたら true、置けなければ false
         */
        public bool TryPlaceAt(int x, int y)
        {
            if (isGameOver) return false;

            if (!IsValidMove(x, y, this.CurrentPlayer))
            {
                // 現在のプレイヤーが合法手を持っていない場合はパス
                if (!HasAnyValidMove(this.CurrentPlayer))
                {
                    var passer = this.CurrentPlayer;
                    this.CurrentPlayer = Opponent(this.CurrentPlayer);
                    this.Board.SetBitBoard(bitBoard);
                    this.DrawBoard(this.Board);
                    this.Notify?.Invoke($"{PlayerName(passer)} passes.");

                    // 相手も合法手がない場合はゲーム終了
                    if (!HasAnyValidMove(this.CurrentPlayer))
                    {
                        NotifyGameOverIfNeeded();
                    }
                }
                return false;
            }

            // Apply move on bitboard
            bitBoard = bitBoard.ApplyMove(this.CurrentPlayer, x, y);

            // プレイヤー交代
            this.CurrentPlayer = Opponent(this.CurrentPlayer);

            // 次のプレイヤーが合法手を持たない場合は再度パスして元に戻す
            if (!HasAnyValidMove(this.CurrentPlayer) && HasAnyValidMove(Opponent(this.CurrentPlayer)))
            {
                this.CurrentPlayer = Opponent(this.CurrentPlayer);
            }

            // 描画更新を先に行い、画面に反映してから通知を出す
            this.Board.SetBitBoard(bitBoard);
            this.DrawBoard(this.Board);

            // 両者とも合法手が無ければゲーム終了
            if (!HasAnyValidMove(Stone.BLACK) && !HasAnyValidMove(Stone.WHITE))
            {
                NotifyGameOverIfNeeded();
            }

            return true;
        }

        /**
         * テスト用途で任意局面を設定する。
         * 
         * - bitBoard と Board を同期してから描画イベントを発火する。
         * - production では通常 InitializeGame()/TryPlaceAt() 経由で局面が進む。
         * 
         * @param bb 設定する局面
         * @param currentPlayer 手番
         */
        internal void SetPositionForTest(BitBoard bb, Stone currentPlayer)
        {
            this.bitBoard = bb;
            this.CurrentPlayer = currentPlayer;
            this.Board.SetBitBoard(bb);
            this.DrawBoard(this.Board);
        }

        /**
         * 指定した色の相手の色を返す。
         * @param s 石の色
         * @return 相手の石の色
         */
        Stone Opponent(Stone s) => s == Stone.BLACK ? Stone.WHITE : Stone.BLACK;

        /**
         * 石の色を人間向けの文字列に変換する。
         * @param s 石の色
         * @return "Black" / "White" / "Empty"
         */
        string PlayerName(Stone s) => s == Stone.BLACK ? "Black" : s == Stone.WHITE ? "White" : "Empty";

        /**
         * 現在のボード上の黒白の石数を数える。
         * @return (black, white) のタプル
         */
        public (int black, int white) CountStones()
        {
            return (bitBoard.PopCountBlack(), bitBoard.PopCountWhite());
        }
    }
}
