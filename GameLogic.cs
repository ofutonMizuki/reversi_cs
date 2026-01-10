using System;
using System.Collections.Generic;
using System.Text;

namespace reversi_cs
{
    /**
     * 石の状態を表す列挙型
     * EMPTY: 空, BLACK: 黒, WHITE: 白
     */
    enum Stone
    {
        EMPTY,
        BLACK,
        WHITE
    }

    /**
     * ボードを表すクラス。8x8 の石の配置を保持する。
     */
    class Board
    {
        // 内部的な2次元配列（ジャグ配列）でボードを保持する
        private Stone[][] field;

        /**
         * コンストラクタ。空のボードを初期化して初期配置を設定する。
         */
        public Board()
        {
            this.field = new Stone[8][];
            for (int i = 0; i < 8; i++)
            {
                this.field[i] = new Stone[8];
                for (int j = 0; j < 8; j++)
                {
                    this.field[i][j] = Stone.EMPTY; // 全マスを空で初期化
                }
            }
            this.Init(); // 初期の4石を置く
        }

        /**
         * ボードを初期状態に戻す。中央に4つの石を配置する。
         */
        public void Init()
        {
            this.field = new Stone[8][];
            for (int i = 0; i < 8; i++)
            {
                this.field[i] = new Stone[8];
                for (int j = 0; j < 8; j++)
                {
                    this.field[i][j] = Stone.EMPTY; // 全マスを空にする
                }
            }

            // 初期配置: (3,3)=BLACK, (3,4)=WHITE, (4,3)=WHITE, (4,4)=BLACK
            field[3][3] = Stone.BLACK;
            field[3][4] = Stone.WHITE;
            field[4][3] = Stone.WHITE;
            field[4][4] = Stone.BLACK;
        }

        /**
         * 指定座標の色を返す。
         * @param x X座標（0-7）
         * @param y Y座標（0-7）
         * @return 指定座標の石の色
         */
        public Stone GetPositionColor(int x, int y)
        {
            // 指定位置の色を返す
            return field[x][y];
        }

        /**
         * 指定座標に色を設定する。
         * @param x X座標（0-7）
         * @param y Y座標（0-7）
         * @param s 設定する色
         */
        public void SetPositionColor(int x, int y, Stone s)
        {
            field[x][y] = s;
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
    }

    /**
     * ゲームのロジックを管理するクラス。
     * 描画コールバックに Board を渡すことができる。
     */
    class GameLogic
    {
        // ゲームのボード
        private Board Board;
        // ボード描画を依頼するコールバック
        readonly Action<Board> DrawBoard;
        // メッセージ表示用のコールバック（パスやゲーム終了通知）
        readonly Action<string> Notify;
        // 現在のプレイヤー
        private Stone CurrentPlayer = Stone.BLACK;

        // 8方向を表すオフセット配列
        private static readonly (int dx, int dy)[] Directions = new (int, int)[]
        {
            (-1, -1), (0, -1), (1, -1),
            (-1, 0),           (1, 0),
            (-1, 1),  (0, 1),  (1, 1)
        };

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
            this.Board.Init();
        }

        /**
         * ゲームを初期化する。ボードを作り直して先手を黒に設定する。
         */
        public void InitializeGame() {
            this.Board = new Board();
            this.CurrentPlayer = Stone.BLACK; // 黒から開始
            this.DrawBoard(this.Board); // 初期状態を描画
        }

        /**
         * 現在のボードを取得する。
         * @return Board オブジェクト
         */
        public Board GetBoard() => this.Board;

        /**
         * 指定方向に裏返しが発生するかを判定し、裏返す座標リストを out で返す。
         * @param x 開始X座標（置こうとしている位置）
         * @param y 開始Y座標
         * @param dx 方向のXオフセット
         * @param dy 方向のYオフセット
         * @param player 現在のプレイヤー
         * @param toFlip 裏返す座標のリスト（out）
         * @return その方向で裏返しが起きるなら true
         */
        bool WouldFlipInDirection(int x, int y, int dx, int dy, Stone player, out List<(int x, int y)> toFlip)
        {
            toFlip = new List<(int x, int y)>();
            int cx = x + dx;
            int cy = y + dy;
            while (Board.InBounds(cx, cy))
            {
                var c = Board.GetPositionColor(cx, cy);
                if (c == Stone.EMPTY) return false; // 空マスが出たら不成立
                if (c == player) return toFlip.Count > 0; // 同色に辿り着き、間に相手石があれば成立
                toFlip.Add((cx, cy)); // 相手石候補を追加
                cx += dx; cy += dy;
            }
            return false; // ボード外に到達したら不成立
        }

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
            if (Board.GetPositionColor(x, y) != Stone.EMPTY) return false; // 空でなければ置けない
            foreach (var (dx, dy) in Directions)
            {
                if (WouldFlipInDirection(x, y, dx, dy, player, out var _)) return true; // いずれかの方向で裏返せれば合法
            }
            return false;
        }

        /**
         * 指定プレイヤーが一つでも合法手を持っているか調べる。
         * @param player 調べるプレイヤー
         * @return 合法手があれば true
         */
        public bool HasAnyValidMove(Stone player)
        {
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    if (IsValidMove(x, y, player)) return true;
            return false;
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
            if (!IsValidMove(x, y, this.CurrentPlayer))
            {
                // 現在のプレイヤーが合法手を持っていない場合はパス
                if (!HasAnyValidMove(this.CurrentPlayer))
                {
                    var passer = this.CurrentPlayer;
                    this.CurrentPlayer = Opponent(this.CurrentPlayer);
                    this.DrawBoard(this.Board);
                    this.Notify?.Invoke($"{PlayerName(passer)} passes."); // パスを通知

                    // 相手も合法手がない場合はゲーム終了
                    if (!HasAnyValidMove(this.CurrentPlayer))
                    {
                        var (b, w) = CountStones();
                        var winner = b > w ? "Black" : (w > b ? "White" : "Draw");
                        this.Notify?.Invoke($"Game Over\nBlack: {b} White: {w}\nWinner: {winner}");
                    }
                }
                return false;
            }

            var allToFlip = new List<(int x, int y)>();
            foreach (var (dx, dy) in Directions)
            {
                if (WouldFlipInDirection(x, y, dx, dy, this.CurrentPlayer, out var flips))
                {
                    allToFlip.AddRange(flips); // 裏返す座標を収集
                }
            }

            // 石を置き、収集した座標を裏返す
            Board.SetPositionColor(x, y, this.CurrentPlayer);
            foreach (var (fx, fy) in allToFlip)
            {
                Board.SetPositionColor(fx, fy, this.CurrentPlayer);
            }

            // プレイヤー交代
            this.CurrentPlayer = Opponent(this.CurrentPlayer);

            // 次のプレイヤーが合法手を持たない場合は再度パスして元に戻す
            if (!HasAnyValidMove(this.CurrentPlayer) && HasAnyValidMove(Opponent(this.CurrentPlayer)))
            {
                this.CurrentPlayer = Opponent(this.CurrentPlayer);
            }

            // 描画更新を先に行い、画面に反映してから通知を出す
            this.DrawBoard(this.Board);

            // 両者とも合法手が無ければゲーム終了
            if (!HasAnyValidMove(Stone.BLACK) && !HasAnyValidMove(Stone.WHITE))
            {
                var (b, w) = CountStones();
                var winner = b > w ? "Black" : (w > b ? "White" : "Draw");
                this.Notify?.Invoke($"Game Over\nBlack: {b} White: {w}\nWinner: {winner}");
            }

            return true;
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
            int b = 0, w = 0;
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    var c = Board.GetPositionColor(x, y);
                    if (c == Stone.BLACK) b++;
                    if (c == Stone.WHITE) w++;
                }
            return (b, w);
        }
    }
}
