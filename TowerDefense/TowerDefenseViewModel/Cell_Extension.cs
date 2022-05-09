using System;
using TowerDefensePersistence;

namespace TowerDefenseViewModel
{
    /// <summary>
    /// Extension class a Cell osztályhoz
    /// </summary>
    public static class Cell_Extension
    {
        /// <summary>
        /// A megadott cella dinamikus típusa alapján visszaadja a megfelelő CellType értéket
        /// </summary>
        /// <param name="cell">A vizsgált cella</param>
        /// <returns>A cella dinamikus típusának megfelelő CellType érték</returns>
        /// <exception cref="Exception">Elérhetetlen kódot jelző kivétel</exception>
        public static CellType GetCellType(this Cell cell)
        {
            if (cell is Plain)
            {
                return CellType.Plain;
            }

            if (cell is Tower)
            {
                if (cell is SupportTower)
                {
                    return (cell as Tower).Player == PlayerType.Player1 ? CellType.Player1SupportTower
                                                                        : CellType.Player2SupportTower;
                }
                if (cell is RangedTower)
                {
                    return (cell as Tower).Player == PlayerType.Player1 ? CellType.Player1RangedTower
                                                                        : CellType.Player2RangedTower;
                }
                if (cell is DamageTower)
                {
                    return (cell as Tower).Player == PlayerType.Player1 ? CellType.Player1DamageTower
                                                                        : CellType.Player2DamageTower;
                }
            }

            if (cell is Water) { return CellType.Water; }
            if (cell is Mountain) { return CellType.Mountain; }



            if (cell is Castle)
            {
                return (cell as Castle).Player == PlayerType.Player1 ? CellType.Player1Castle
                                                                     : CellType.Player2Castle;
            }

            throw new Exception(); //unreachable code
        }

    }
}
