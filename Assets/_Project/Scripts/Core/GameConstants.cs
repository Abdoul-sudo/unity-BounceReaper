namespace BounceReaper
{
    public static class GameConstants
    {
        // Layers
        public const string LayerBall = "Ball";
        public const string LayerEnemy = "Enemy";
        public const string LayerWall = "Wall";

        // Sorting Orders
        public const int SortOrderBackground = 0;
        public const int SortOrderEnemies = 10;
        public const int SortOrderBalls = 20;
        public const int SortOrderVFX = 30;
        public const int SortOrderDamageNumbers = 40;
        public const int SortOrderUI = 100;

        // Save
        public const string SaveFileName = "save.json";
        public const int SaveVersion = 1;

        // Physics
        public const int TargetFrameRateDefault = 60;
    }
}
