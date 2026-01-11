using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace reversi_cs
{
    /**
     * ゲームウィンドウを表すフォームクラス。
     * PictureBox にボードを描画し、クリックで石を置く。
     */
    public class GameForm : Form
    {
        // 描画用 PictureBox
        private readonly PictureBox field = new PictureBox();
        // ゲームロジック
        private GameLogic game;
        // 現在表示しているボード参照
        private Board board;

        private readonly GameConfig config;

        private readonly RandomPlayer blackRandom = new RandomPlayer(Stone.BLACK);
        private readonly RandomPlayer whiteRandom = new RandomPlayer(Stone.WHITE);

        private readonly int alphaBetaDepth;
        private readonly AlphaBetaSearch? alphaBetaSearch;

        private readonly System.Windows.Forms.Timer aiTimer = new System.Windows.Forms.Timer();

        /**
         * コンストラクタ。フォームの初期化とイベント登録を行う。
         */
        public GameForm(GameConfig? config = null)
        {
            this.config = config ?? new GameConfig();
            this.alphaBetaDepth = Math.Max(1, this.config.AlphaBetaDepth);

            this.Text = "Game";
            this.ClientSize = new Size(640, 640);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            field.Dock = DockStyle.Fill;

            this.Controls.Add(field);

            // Paint と MouseClick を一度だけ登録する
            field.Paint += Field_Paint;
            field.MouseClick += Field_MouseClick;

            // GameLogic に描画コールバックと通知コールバックを渡す
            game = new GameLogic(this.DrawBoard, this.Notify);
            game.InitializeGame();

            aiTimer.Interval = 200;
            aiTimer.Tick += (_, __) =>
            {
                aiTimer.Stop();
                TryAIMoveIfNeeded();
            };

            // If black is AI, start immediately.
            if (!IsHumanTurn())
                aiTimer.Start();

            // AlphaBetaNN: load model once
            if (this.config.Black == PlayerType.AlphaBetaNN || this.config.White == PlayerType.AlphaBetaNN)
            {
                var modelPath = ModelPathResolver.Resolve();
                var eval = NNEvaluator.LoadFromFile(modelPath);
                this.alphaBetaSearch = new AlphaBetaSearch(eval);
            }

            this.Activate();
        }

        private bool IsHumanTurn()
        {
            var current = game.GetCurrentPlayer();
            var currentType = current == Stone.BLACK ? config.Black : config.White;
            return currentType == PlayerType.Human;
        }

        /**
         * フォームが閉じられたときの処理。オーナーを再有効化する。
         */
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            this.Owner?.Enabled = true;
            this.Owner?.Activate();
        }

        /**
         * マウスクリックイベントハンドラ。
         * クリック位置からセル座標を計算して石を置く処理を呼ぶ。
         * @param sender イベント送信元
         * @param e マウスイベント引数
         */
        private void Field_MouseClick(object? sender, MouseEventArgs e)
        {
            if (board == null) return;
            if (game.IsGameOver()) return;
            if (!IsHumanTurn()) return;

            int cellSize = this.ClientSize.Width / 8;
            int x = Math.Clamp(e.X / cellSize, 0, 7);
            int y = Math.Clamp(e.Y / cellSize, 0, 7);

            // GameLogic に石を置く要求を送る（結果は内部で描画更新される）
            _ = game.TryPlaceAt(x, y);

            // let AI respond
            if (!IsHumanTurn() && !aiTimer.Enabled)
                aiTimer.Start();
        }

        private void TryAIMoveIfNeeded()
        {
            if (game.IsGameOver())
            {
                aiTimer.Stop();
                return;
            }

            if (IsHumanTurn()) return;

            var current = game.GetCurrentPlayer();
            var currentType = current == Stone.BLACK ? config.Black : config.White;

            (int x, int y)? move;

            if (currentType == PlayerType.AlphaBetaNN)
            {
                if (alphaBetaSearch is null) return;
                var pos = game.GetBoard().GetBitBoard();
                move = alphaBetaSearch.FindBestMove(pos, current, alphaBetaDepth);
            }
            else
            {
                RandomPlayer ai = current == Stone.BLACK ? blackRandom : whiteRandom;
                move = ai.ChooseMove(game);
            }

            if (move is null)
            {
                _ = game.TryPlaceAt(-1, -1);
                return;
            }

            _ = game.TryPlaceAt(move.Value.x, move.Value.y);

            // Continue while AI still has the turn (e.g., opponent pass).
            if (game.IsGameOver())
            {
                aiTimer.Stop();
                return;
            }

            if (!IsHumanTurn() && !aiTimer.Enabled)
                aiTimer.Start();
        }

        /**
         * 描画イベントハンドラ。格子と石を描画する。
         * @param sender イベント送信元
         * @param e ペイントイベント引数
         */
        private void Field_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Green); // 背景を緑にする
            Pen pen = new Pen(Color.Black, 2);
            int cellSize = this.ClientSize.Width / 8;

            // グリッド線を描く
            for (int i = 0; i <= 8; i++)
            {
                g.DrawLine(pen, i * cellSize, 0, i * cellSize, this.ClientSize.Height);
                g.DrawLine(pen, 0, i * cellSize, this.ClientSize.Width, i * cellSize);
            }

            if (board == null) return;

            // 石を描画する
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Stone stone = board.GetPositionColor(x, y);
                    if (stone != Stone.EMPTY)
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias; // アンチエイリアス有効
                        Brush brush = (stone == Stone.BLACK) ? Brushes.Black : Brushes.White;
                        g.FillEllipse(brush, x * cellSize + 4, y * cellSize + 4, cellSize - 8, cellSize - 8);
                    }
                }
            }
        }

        /**
         * 描画用コールバック。GameLogic から呼ばれてボード参照を更新し再描画する。
         * @param board 描画対象の Board
         */
        void DrawBoard(Board board)
        {
            // ローカルにボードを保持して PictureBox を即時再描画する
            this.board = board;
            // Invalidate() は非同期なので、通知ダイアログより前に描画を反映させるため Refresh() を使う
            field.Refresh();
        }

        /**
         * 通知用コールバック。GameLogic からのメッセージを MessageBox で表示する。
         * UI スレッドでない場合は BeginInvoke でメインスレッドに委譲する。
         * @param message 表示するメッセージ
         */
        void Notify(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => MessageBox.Show(this, message, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)));
            }
            else
            {
                MessageBox.Show(this, message, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
