using System;
using TowerDefensePersistence;

namespace TowerDefenseModel
{
    /// <summary>
    /// Játék vége esetén tartalmazza a győztest
    /// </summary>
    public class GameOverEventArgs : EventArgs
    {
        PlayerType winner;

        public PlayerType Winner { get => winner; set => winner = value; }
    }
}
