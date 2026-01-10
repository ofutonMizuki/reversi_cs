namespace reversi_cs
{
    public sealed class GameConfig
    {
        public PlayerType Black { get; init; } = PlayerType.Human;
        public PlayerType White { get; init; } = PlayerType.Random;
    }
}
