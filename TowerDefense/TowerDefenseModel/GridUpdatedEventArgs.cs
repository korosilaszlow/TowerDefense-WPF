using System;
using System.Collections.Generic;

namespace TowerDefenseModel
{
    /// <summary>
    /// Tartalmazza a frissítendő cellákat, valamint, hogy milyen fázisban történik a frissítés
    /// </summary>
    public class GridUpdatedEventArgs : EventArgs
    {

        private List<CellUpdate> updates;
        private GamePhase gamePhase;
        public GridUpdatedEventArgs(GamePhase phase)
        {
            gamePhase = phase;
            updates = new List<CellUpdate>();
        }

        public GamePhase GamePhase { get => gamePhase; set => gamePhase = value; }
        public List<CellUpdate> Updates { get => updates; set => updates = value; }
    }
}
