# reversi_cs

C#（Windows Forms）で実装したリバーシ（オセロ）です。8×8 の盤面で、合法手判定・反転処理・パス・終局判定までを含みます。

## 動作環境
- .NET 10
- Windows（`net10.0-windows` / Windows Forms）

## ビルド
```sh
dotnet build
```

## 実行
```sh
dotnet run --project ./reversi_cs.csproj
```

## ゲーム概要
- 盤面は 8×8。
- 石を置くと、上下左右・斜めの 8 方向について「相手の石を挟めた列」が反転します。
- 合法手がない場合はパスします。
- 両者とも合法手がなくなった時点で終局し、黒白の数を数えて勝敗を表示します。

## 操作方法
1. 起動すると設定画面が表示されます。
2. 黒・白それぞれのプレイヤー種別を選択します。
3. `Start Game` を押すとゲーム画面が開きます。
4. 人間操作の手番では、盤面をクリックして石を置きます。

## プレイヤー種別
- `Human`: クリック操作で手を選択します。
- `Random`: 合法手の中からランダムに 1 手選びます（簡易 AI）。

## 実装の概要

### 画面（UI）
- 設定画面（`Form1`）
  - 黒・白のプレイヤー種別（`Human` / `Random`）を選択してゲーム開始します。
- ゲーム画面（`GameForm`）
  - `PictureBox` に盤面を描画します。
  - クリック位置からセル（0-7, 0-7）を計算して着手処理を呼び出します。
  - AI 手番は `Timer` で一定間隔で手を進めます（連続手番＝相手パスにも対応）。

### ルール（ロジック）
- `GameLogic`
  - 合法手判定（`IsValidMove`）
  - 8 方向の走査による反転判定（`WouldFlipInDirection`）
  - 着手処理（`TryPlaceAt`）
  - パス処理（合法手なしの場合）
  - 終局判定（両者合法手なし）と石数カウント（`CountStones`）
  - ビットボード（`BitBoard`）を併用し、合法手一覧生成（`GetValidMoves`）と石数カウントを高速化
- `Board`
  - 8×8 の盤面状態を保持します。
  - 初期配置（中央 4 石）を設定します。
- `Stone`
  - マスの状態（`EMPTY` / `BLACK` / `WHITE`）を表す列挙型です。

### ビットボード（高速化の下地）
- `BitBoard`
  - 8×8 盤面を `ulong` 2 本（黒/白）で表現します（index = `y*8 + x`）。
  - 合法手生成（ビットマスク）や反転ビット計算などの高速処理を実装しています。
  - 現状は UI 互換のため `Board` と同期して使い、将来的な最適化のための基盤として導入しています。

### NN評価関数（探索用の下地）
- `NNEvaluator`
  - TypeScript 版の NN 評価関数（`NNEval`）を参考にした C# 実装です。
  - JSON から重みを読み込み、`BitBoard` の局面を実数スコアに評価します（手番視点）。
  - 現時点では探索AIへの組み込みは行っておらず、評価関数クラスのみ追加しています。
  - モデルファイル（重みJSON）の配置について:
    - 本リポジトリにはモデルJSONは同梱しません（別途用意）。
    - 実行時にローカルファイルパスを指定して読み込みます。
    - 例: `var eval = NNEvaluator.LoadFromFile(@"C:\path\to\model.json");`
      - 相対パスを使う場合は「実行時のカレントディレクトリ」基準になります（`dotnet run` では通常プロジェクトディレクトリ）。
  - 使い方例:
    - `var eval = NNEvaluator.LoadFromFile("path/to/model.json");`
    - `double score = eval.Evaluate(board.GetBitBoard(), sideToMove, perspectiveColor);`

## ファイル構成
- `Program.cs`: エントリーポイント（`Form1` を起動）
- `Form1.cs`: 設定画面（プレイヤー種別の選択）
- `Form1.Designer.cs`: `Form1` のデザイナ生成コード
- `GameForm.cs`: ゲーム画面（描画・クリック処理・AI タイマー）
- `GameLogic.cs`: ルール・盤面管理
- `RandomPlayer.cs`: ランダム手の簡易 AI
- `GameConfig.cs`: ゲーム設定
- `PlayerType.cs`: プレイヤー種別
- `BitBoard.cs`: ビットボード表現と合法手生成など（高速化の下地）
- `NNEvalModel.cs`: NN評価関数モデル（JSON読み込み用DTO）
- `NNEvaluator.cs`: NN評価関数（前向き計算）

## 開発補助ツール
このプロジェクトの作成・修正には `GitHub Copilot` を使用しています。

## ライセンス
リポジトリ内のライセンスファイルを参照してください（存在する場合）。
