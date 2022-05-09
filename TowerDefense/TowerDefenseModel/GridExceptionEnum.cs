namespace TowerDefenseModel
{
    /// <summary>
    /// A cellák lerakásával kapcsolatos hibák
    /// </summary>
    public enum GridExceptionEnum
    {
        NO_PROBLEM,
        MONEY,
        INVALID_CELL,
        SOLDIER_PATH_CUT,
        ENEMY_CASTLE_TOO_CLOSE,
        NO_NEARBY_ALLY_TOWER,
        INVALID_GAME_STATE,
        TOWER_MAX_LEVEL
    }
}
