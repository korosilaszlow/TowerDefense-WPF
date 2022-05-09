using System;

namespace TowerDefenseModel
{
    /// <summary>
    /// A cellák lerakásával kapcsolatos hiba esetén dobódik (a pontos hibákat a GridExceptionEnum tartalmazza)
    /// </summary>
    public class GridException : Exception
    {
        private GridExceptionEnum msg;
        public GridExceptionEnum Msg { get { return msg; } }
        public GridException(GridExceptionEnum message)
        {
            msg = message;
        }
    }
}
