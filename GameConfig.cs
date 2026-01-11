namespace reversi_cs
{
    /**
     * ƒQ[ƒ€İ’èB
     */
    public sealed class GameConfig
    {
        public PlayerType Black { get; init; } = PlayerType.Human;
        public PlayerType White { get; init; } = PlayerType.Random;

        /**
         * AlphaBetaNN ‚Ì’Tõ[‚³iplyjB
         */
        public int AlphaBetaDepth { get; init; } = 4;
    }
}
