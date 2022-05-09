using System;
using TowerDefensePersistence;

namespace TowerDefenseModel
{
    /// <summary>
    /// Vektor távolság számítása
    /// </summary>
    public static class Distance
    {
        /// <summary>
        /// Manhattan norma két cella között
        /// </summary>
        /// <param name="a">Első cella</param>
        /// <param name="b">Második cella</param>
        /// <returns>Két cella közötti Manhattan normát</returns>
        public static int Manhattan(Cell a, Cell b)
        {
            return Manhattan(CellToCoords(a), CellToCoords(b));
        }

        /// <summary>
        /// Chebyshev norma két cella között
        /// </summary>
        /// <param name="a">Első cella</param>
        /// <param name="b">Második cella</param>
        /// <returns>Két cella közötti Chebyshev normát</returns>
        public static int Chebyshev(Cell a, Cell b)
        {
            return Chebyshev(CellToCoords(a), CellToCoords(b));
        }

        /// <summary>
        /// Manhattan norma két koordináta pár között
        /// </summary>
        /// <param name="a">Első koordináta pár</param>
        /// <param name="b">Második koordináta pár</param>
        /// <returns>Két koordináta pár közötti Manhattan normát</returns>
        public static int Manhattan((int x, int y) a, (int x, int y) b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        /// <summary>
        /// Chebyshev norma két koordináta pár között
        /// </summary>
        /// <param name="a">Első koordináta pár</param>
        /// <param name="b">Második koordináta pár</param>
        /// <returns>Két koordináta pár közötti Chebyshev normát</returns>
        public static int Chebyshev((int x, int y) a, (int x, int y) b)
        {
            return Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
        }

        /// <summary>
        /// Egy cellát egy koordináta párra képez
        /// </summary>
        /// <param name="c">Cella</param>
        /// <returns>Cella koordinátái párba szervezve</returns>
        public static (int x, int y) CellToCoords(Cell c)
        {
            return (c.Row, c.Col);
        }


    }
}
