namespace BounceReaper
{
    public enum GameState
    {
        None = 0,
        Boot,
        MainMenu,
        Loading,
        Playing,
        Pause,
        GameOver
    }

    public enum TurnPhase
    {
        None = 0,
        Aiming,
        Firing,
        Resolving,
        EnemyPhase,
        CheckEnd
    }
}
