using System;

namespace reversi_cs
{
    public sealed class RandomPlayer
    {
        private readonly Stone stone;
        private readonly Random rng;

        public RandomPlayer(Stone stone, Random? rng = null)
        {
            this.stone = stone;
            this.rng = rng ?? new Random();
        }

        public Stone Stone => this.stone;

        public (int x, int y)? ChooseMove(GameLogic game)
        {
            if (game is null) throw new ArgumentNullException(nameof(game));
            return game.GetRandomValidMove(this.stone, this.rng);
        }
    }
}
